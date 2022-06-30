using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Configs;
using AQS.OrderProject.Domain.Customers;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Exceptions;
using AQS.OrderProject.Domain.Reponses;
using AQS.OrderProject.Domain.SharedData;
using Microsoft.Extensions.Options;
using Navyblue.BaseLibrary;
using Serilog;
using TFA.AQS.Order.Application;

namespace AQS.OrderProject.Application.Orders.V3.PlaceOrder
{
    public class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand, AqsResponse>
    {
        private readonly WebsiteConfig _websiteConfig;
        private readonly IOrdersRepository _ordersRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IRedisRepository _redisRepository;
        private readonly ILogger _logger;
        private readonly GameConfig _gameConfig;
        private readonly IOrderDuplicateChecker _orderDuplicateChecker;

        public PlaceOrderCommandHandler(IOrdersRepository ordersRepository,
            ITasksRepository tasksRepository,
            IRedisRepository redisRepository,
            ILogger logger,
            IOptions<WebsiteConfig> webSiteConfig,
            IOptions<GameConfig> gameConfig,
            IOrderDuplicateChecker orderDuplicateChecker)
        {
            this._websiteConfig = webSiteConfig.Value;
            this._ordersRepository = ordersRepository;
            this._tasksRepository = tasksRepository;
            _redisRepository = redisRepository;
            this._logger = logger;
            _orderDuplicateChecker = orderDuplicateChecker;
            this._gameConfig = gameConfig.Value;
        }

        public async Task<AqsResponse> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
        {
            var userCompany = _websiteConfig.Websites.FirstOrDefault(p => p.UserName == command.UserName)?.Name;

            Log.Information("[PlaceOrder] OrderId: <{}> prepare to process place order request from user: <{}>, company: <{}>",
                    command.OrderId, command.UserName, userCompany);

            string agentOrderId;

            // Check Order is existed or not by orderId
            var orderInDb = await this._ordersRepository.GetOrderByOrderId(command.UserName, command.OrderId);
            if (orderInDb != null)
            {
                agentOrderId = orderInDb.AgentId;

                Log.Information("[PlaceOrder] OrderId: <{}> find mapping order in database, agentOrderId: <{}>",
                        command.OrderId, orderInDb.AgentId);

                // Update table: orders, fields: Market, MarketType, OrderMappings, OrderStatus, UpdateTime
                UpdateOrderInDbFields(command, orderInDb);

                await this._ordersRepository.UpdateOrderFields(command.UserName, 
                    orderInDb.OrderId, 
                    orderInDb.Market, 
                    orderInDb.MarketType, 
                    orderInDb.OrderMappings, 
                    orderInDb.OrderLines,
                    orderInDb.OrderStatus);

                Log.Information("OrderId: <{}> update orders fields market: <{}>, marketType: <{}>, orderMappings: <{}>, orderStatus: <{}>, updateTime: <{}> done",
                    orderInDb.OrderId, 
                    orderInDb.Market, 
                    orderInDb.MarketType, 
                    orderInDb.OrderMappings,
                    orderInDb.OrderStatus, 
                    orderInDb.UpdateTime);

                // Update table: tasks, fields: MarketType, BetType
                var marketType = GetMarketTypeFromReq(command);
                var betType = await GetBetTypeFromReq(marketType, command);

                await this._tasksRepository.UpdateTaskMarketTypeAndBetType(command.UserName, orderInDb.TaskId, marketType, betType);

                Log.Information(
                    "OrderId: <{}> update taskId: <{}> fields marketType: <{}>, BetType: <{}> done",
                    orderInDb.OrderId, orderInDb.TaskId, marketType, betType);

                // Update redis: task, fields: MarketType, BetType, Stake, OrderLines
                var taskCacheModelFromRedis = new TaskCacheDto();
                var taskFromRedis = await _redisRepository.get<TaskCacheDto>(RedisCacheCategory.Task, userCompany, orderInDb.TaskId.ToString());

                _logger.Information("OrderId: <{}> get task from redis succeed, taskId: <{}>, content: {}", command.OrderId, orderInDb.TaskId,
                    taskCacheModelFromRedis);
                if (taskFromRedis != null)
                {
                    // Set MarketType
                    taskFromRedis.MarketType = marketType;
                    // Set BetType
                    taskFromRedis.BetType = betType;
                    // Set Stake
                    taskFromRedis.Stake = command.Stake;

                    // Set OrderLines
                    var cacheOrderLines = UkOrderLinesPriceConvert(command.OrderLines);
                    taskFromRedis.OrderLines = cacheOrderLines;

                    // Update redis cache
                    var taskId = taskFromRedis.Id;
                    _redisRepository.put<TaskCacheDto>(RedisCacheCategory.Task, userCompany, taskId.ToString(), taskFromRedis);
                    _logger.Information("OrderId: <{}> update task at redis succeed, taskId: <{}>, content: {}", command.OrderId, taskId,
                        taskFromRedis.ToJson());

                    // Notify eds new order update
                    orderInDb = await this._ordersRepository.GetOrderByOrderId(command.UserName, command.OrderId);
                    //notifyEdsUpdateOrder(req.OrderId, req.UserName, orderInDB);
                }
                else
                {
                    Log.Warning("[PlaceOrder] OrderId: <{}> cannot find mapping task from redis with taskId: <{}>, please check...",
                            command.OrderId, orderInDb.TaskId);
                }
            }
            else
            {
                // 用來讓 UK 知道我們處理這筆的系統 id
                agentOrderId = Guid.NewGuid().ToString();

                // Extract order and reference match
                var orderAndRefMatch = await GetOrderAndRefMatchFromPlaceOrder(command.UserName, agentOrderId, command);

                var order = orderAndRefMatch.Item1;
                var refMatch = orderAndRefMatch.Item2;

                // Check task is duplicated or not
                var taskDuplicated = await this._tasksRepository.CheckTaskDuplicated(command.UserName, order.MatchMapId, order.MarketType);
                if (taskDuplicated)
                {
                    Log.Warning("[PlaceOrder] Duplicated task, orderId: <{}>, matchMapId: <{}>, marketType: <{}>, agentOrderId: <{}>",
                            command.OrderId, order.MatchMapId, order.MarketType, agentOrderId);

                    var message = "Duplicated order orderId {0}, CreatedTimeUtc {1}, agentOrderId {2}";

                    throw new Exception(message.FormatWith(command.OrderId, command.CreatedTime, agentOrderId));
                }

                var seq = await this._tasksRepository.GetNextSeq(command.UserName, order.CreatedTime);

                TasksModel task = new()
                {
                    Seq = seq > 0 ? seq : 1,
                    MatchMapId = order.MatchMapId,
                    MarketType = order.MarketType,
                    TaskStatus = OrderTaskStatus.Ship,
                    Notified = false,
                    CreatedDate = order.CreatedTime,
                    CreatedBy = order.CreatedBy,
                    BetType = order.BetType
                };

                //bool isDuplicated = 
                var tasks = await this._tasksRepository.InsertTask(command.UserName, task.Seq, task.MatchId, task.MatchMapId, task.MarketType, task.TaskStatus, task.Notified, task.CreatedDate,
                    task.CreatedBy, task.BetType, task.FinishTime, task.FinishBy);

                _logger.Information("OrderId: <{}> insert task done, result: {}", command.OrderId, tasks?.ToJson());

                order.SetTaskId(task.Id);

                await this._ordersRepository.InsertOrder(command.UserName, order);

                Log.Information("OrderId: <{}> insert order done, result: {}", command.OrderId, order.ToJson());

                var taskCacheModel = this.ConvertToTaskCacheModel(order, task, refMatch, command);

                _redisRepository.put(RedisCacheCategory.Task, userCompany, taskCacheModel.Id.ToString(), taskCacheModel);

                _logger.Information("OrderId: <{}> insert task to redis done, result: {}", command.OrderId, taskCacheModel.ToJson());

                //notifyEdsNewOrderComing(req.getOrderId(), req.UserName, order);
            }

            var succeedResp = CreateSucceedResp(command.OrderId.Value.ToString(), command.CreatedTime, agentOrderId);

            Log.Information("[PlaceOrder] OrderId: <{}> process place order request done, response: {}",
                    command.OrderId, succeedResp.ToJson());

            return succeedResp;
        }

        protected AqsResponse CreateSucceedResp(string orderId, DateTime createdTime, string agentOrderId)
        {
            AqsResponse succeedResp = new()
            {
                OrderId = orderId,
                CreatedTimeUtc = createdTime.ToString("yyyy-MM-dd HH:mm:ss"),
                AgentOrderId = agentOrderId,
                Code = (int)HttpStatusCode.OK,
                Message = "OK"
            };

            return succeedResp;
        }

        private async Task<Tuple<Order, Matches>> GetOrderAndRefMatchFromPlaceOrder(string currentUser, string agentOrderId, PlaceOrderCommand request)
        {
            var hgOrderMapping = request.OrderMappings.FirstOrDefault(o => o.Book == AqsBook.Singbet);
            var sboOrderMapping = request.OrderMappings.FirstOrDefault(o => o.Book == AqsBook.Sbobet);
            var ibcOrderMapping = request.OrderMappings.FirstOrDefault(o => o.Book == AqsBook.Ibcbet);

            var tuple = await MatchProvider.ProcessMatch(request.OrderId,
                _gameConfig.Host,
                request.Phase == AqsMarket.InRunning,
                hgOrderMapping,
                sboOrderMapping,
                ibcOrderMapping,
                _logger);

            var refOrderMapping = tuple.Item1;
            var refMatch = tuple.Item2;

            if (refMatch != null)
            {
                var order = Order.PlaceOrder(currentUser,
                    request.OrderId,
                    MatchType.Normal, // FIXME EDS order 的 match type 可能要根據 UK 來的 Score Type 來決定
                    request.OrderMappings.ToJson(),
                    null,
                    "v3",
                    refMatch.MatchMapId,
                    OrderStatus.Running,
                    0,
                    agentOrderId,
                    refMatch.Score,
                    request.HitterId,
                    request.CreatedTime.ToGmt0Time(),
                    request.CreatedTime.ToGmt0Time(),
                    request.Phase,
                    request.Stage,
                    request.MarketType,
                    refOrderMapping.Choice,
                    refOrderMapping.HomeTeam,
                    refOrderMapping.AwayTeam,
                    _orderDuplicateChecker
                );

                return new Tuple<Order, Matches>(order, refMatch);
            }

            AddNoMappingToDB(currentUser, agentOrderId, request, hgOrderMapping, sboOrderMapping, ibcOrderMapping);

            return null;
        }

        /// <summary>
        /// // 若 refMatch is null, 代表都找不到對應賽事, 丟出 DataNotFoundException
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="agentOrderId"></param>
        /// <param name="request"></param>
        /// <param name="hgOrderMapping"></param>
        /// <param name="sboOrderMapping"></param>
        /// <param name="ibcOrderMapping"></param>
        /// <returns></returns>
        /// <exception cref="DataNotFoundException"></exception>
        private void AddNoMappingToDB(string userName, string agentOrderId, PlaceOrderCommand request,
            OrderMapping hgOrderMapping, OrderMapping sboOrderMapping, OrderMapping ibcOrderMapping)
        {
            Order.PlaceUkNotMappingOrder(userName, request.OrderId,
                request.CreatedTime.ToGmt0Time(),
                request.ToJson(),
                hgOrderMapping?.ScheduledKickOffTimeUtc,
                hgOrderMapping?.Tournament,
                hgOrderMapping?.HomeTeam,
                hgOrderMapping?.AwayTeam,
                sboOrderMapping?.ScheduledKickOffTimeUtc,
                sboOrderMapping?.Tournament,
                sboOrderMapping?.HomeTeam,
                sboOrderMapping?.AwayTeam,
                ibcOrderMapping?.ScheduledKickOffTimeUtc,
                ibcOrderMapping?.Tournament,
                ibcOrderMapping?.HomeTeam,
                ibcOrderMapping?.AwayTeam);

            var errorMsgBuffer = CreateErrorMessage(request, hgOrderMapping, sboOrderMapping, ibcOrderMapping);
            var message = errorMsgBuffer.ToString();

            throw new DataNotFoundException(request.OrderId.Value.ToGuidString(), request.CreatedTime, agentOrderId, message);
        }

        private StringBuilder CreateErrorMessage(PlaceOrderCommand request, OrderMapping hgOrderMapping,
            OrderMapping sboOrderMapping, OrderMapping ibcOrderMapping)
        {
            var errorMsgBuffer = new StringBuilder();
            errorMsgBuffer.Append("Cannot find mapping ");
            if (hgOrderMapping != null)
            {
                errorMsgBuffer
                    .Append("HG match with ")
                    .Append("Tournament: ").Append(hgOrderMapping.Tournament).Append(", ")
                    .Append("HomeTeam: ").Append(hgOrderMapping.HomeTeam).Append(", ")
                    .Append("AwayTeam: ").Append(hgOrderMapping.AwayTeam).Append(", ")
                    .Append("ScheduledKickOffTimeUtc: ").Append(hgOrderMapping.ScheduledKickOffTimeUtc).Append("; ");
            }

            if (sboOrderMapping != null)
            {
                errorMsgBuffer
                    .Append("SBO match with ")
                    .Append("Tournament: ").Append(sboOrderMapping.Tournament).Append(", ")
                    .Append("HomeTeam: ").Append(sboOrderMapping.HomeTeam).Append(", ")
                    .Append("AwayTeam: ").Append(sboOrderMapping.AwayTeam).Append(", ")
                    .Append("ScheduledKickOffTimeUtc: ").Append(sboOrderMapping.ScheduledKickOffTimeUtc).Append("; ");
            }

            if (ibcOrderMapping != null)
            {
                errorMsgBuffer
                    .Append("IBC match with ")
                    .Append("Tournament: ").Append(ibcOrderMapping.Tournament).Append(", ")
                    .Append("HomeTeam: ").Append(ibcOrderMapping.HomeTeam).Append(", ")
                    .Append("AwayTeam: ").Append(ibcOrderMapping.AwayTeam).Append(", ")
                    .Append("ScheduledKickOffTimeUtc: ").Append(ibcOrderMapping.ScheduledKickOffTimeUtc).Append("; ");
            }

            _logger.Warning(LogResource.V2POCH03.FormatWith(request.OrderId, hgOrderMapping, sboOrderMapping, ibcOrderMapping));
            return errorMsgBuffer;
        }

        private void UpdateOrderInDbFields(PlaceOrderCommand req, Order orderInDb)
        {
            // Market
            var phase = req.Phase;
            var market = Market.None;
            switch (phase)
            {
                case AqsMarket.PreMatch:
                    market = Market.Today;
                    break;

                case AqsMarket.InRunning:
                    market = Market.Live;
                    break;
            }

            // MarketType
            var stage = req.Stage;
            var marketType = MarketType.None;

            switch (stage)
            {
                case AqsStage.FirstHalf:
                    switch (req.MarketType)
                    {
                        case AqsMarketType.AsianHandicap:
                            marketType = MarketType.HtHdp;
                            break;

                        case AqsMarketType.OverUnder:
                            marketType = MarketType.HtOu;
                            break;
                    }
                    break;

                case AqsStage.FirstHalfExtraTime:
                    // FIXME EDS Order 的 MatchType 應該要根據 UK stage 塞入, 目前系統沒 FirstHalfExtraTime
                    break;

                case AqsStage.FullTime:
                    switch (req.MarketType)
                    {
                        case AqsMarketType.AsianHandicap:
                            marketType = MarketType.FtHdp;
                            break;

                        case AqsMarketType.OverUnder:
                            marketType = MarketType.FtOu;
                            break;
                    }
                    break;

                case AqsStage.FullTimeExtraTime:
                    // FIXME EDS Order 的 MatchType 應該要根據 UK stage 塞入, 目前系統沒 FullTimeExtraTime
                    break;

                case AqsStage.Penalties:
                    // FIXME EDS Order 的 MatchType 應該要根據 UK stage 塞入, 目前系統沒 Penalties
                    break;
            }

            orderInDb.Change(market,
                marketType,
                req.OrderMappings.ToJson(),
                req.OrderLines?.Count == 0 ? orderInDb.OrderLines : req.OrderLines.ToJson(),
                OrderStatus.Running,orderInDb.CreatedBy);
        }


        private MarketType GetMarketTypeFromReq(PlaceOrderCommand req)
        {
            var result = MarketType.None;

            var reqStage = req.Stage;
            var reqMarketType = req.MarketType;

            switch (reqStage)
            {
                case AqsStage.FirstHalf:
                    switch (reqMarketType)
                    {
                        case AqsMarketType.AsianHandicap:
                            result = MarketType.HtHdp;
                            break;

                        case AqsMarketType.OverUnder:
                            result = MarketType.HtOu;
                            break;
                    }
                    break;

                case AqsStage.FirstHalfExtraTime:
                    // FIXME EDS Order 的 MatchType 應該要根據 UK stage 塞入, 目前系統沒 FirstHalfExtraTime
                    break;

                case AqsStage.FullTime:
                    switch (reqMarketType)
                    {
                        case AqsMarketType.AsianHandicap:
                            result = MarketType.FtHdp;
                            break;

                        case AqsMarketType.OverUnder:
                            result = MarketType.FtOu;
                            break;
                    }
                    break;

                case AqsStage.FullTimeExtraTime:
                    // FIXME EDS Order 的 MatchType 應該要根據 UK stage 塞入, 目前系統沒 FullTimeExtraTime
                    break;

                case AqsStage.Penalties:
                    // FIXME EDS Order 的 MatchType 應該要根據 UK stage 塞入, 目前系統沒 Penalties
                    break;
            }
            return result;
        }

        protected async Task<OrderMapping> GetNewHgAqsOrderMapping(OrderMapping hgOrderMapping, OrderMapping sboOrderMapping, OrderMapping ibcOrderMapping)
        {
            ArrayList aqsOrderMappings = new();

            if (hgOrderMapping != null)
            {
                aqsOrderMappings.Add(hgOrderMapping);
            }

            if (sboOrderMapping != null)
            {
                aqsOrderMappings.Add(sboOrderMapping);
            }

            if (ibcOrderMapping != null)
            {
                aqsOrderMappings.Add(ibcOrderMapping);
            }

            var aqsOrderMappingArr = new OrderMapping[aqsOrderMappings.Count];
            aqsOrderMappings.CopyTo(aqsOrderMappingArr);
            Keyword keyword = new(aqsOrderMappingArr);

            _logger.Information("[GameSearching] Got Keyword: {}", keyword.ToJson());

            var searchResult = await GetSearchResult(keyword);

            OrderMapping newOrderMapping = new()
            {
                Tournament = searchResult.League,
                HomeTeam = searchResult.Home,
                AwayTeam = searchResult.Away,
                Book = hgOrderMapping.Book,
                Choice = searchResult.BetType,
                LiveHomeScore = searchResult.HomeScore,
                LiveAwayScore = searchResult.AwayScore,
                ScheduledKickOffTimeUtc = searchResult.GameTimeUtc
            };

            _logger.Information("[GameSearching] Got new AQSOrderMapping: {}", newOrderMapping.ToString());
            return newOrderMapping;
        }

        private async Task<SearchResult> GetSearchResult(Keyword keyword)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_gameConfig.Host);
            var response = await httpClient.PostAsJsonAsync("/api/match/search/1", keyword);
            var result = await response.Content.ReadAsStringAsync();
            var searchResult = result.FromJson<SearchResult>();

            _logger.Information($"[GameSearching] Searching url=<{httpClient.BaseAddress + "/api/match/search/1"}> got result: {searchResult.ToJson()}");
            return searchResult;
        }

        private async Task<BetType> GetBetTypeFromReq(MarketType marketType, PlaceOrderCommand req)
        {
            var hgOrderMapping = req.OrderMappings.FirstOrDefault(o => o.Book == AqsBook.Singbet);
            var sboOrderMapping = req.OrderMappings.FirstOrDefault(o => o.Book == AqsBook.Sbobet);
            var ibcOrderMapping = req.OrderMappings.FirstOrDefault(o => o.Book == AqsBook.Ibcbet);
            hgOrderMapping = await GetNewHgAqsOrderMapping(hgOrderMapping, sboOrderMapping, ibcOrderMapping);

            // 依據 hg -> sbo -> ibc 順序, 不為 null 就拿他當參考物件
            OrderMapping refOrderMapping = null;
            if (hgOrderMapping != null)
            {
                refOrderMapping = hgOrderMapping;
            }
            else if (sboOrderMapping != null)
            {
                refOrderMapping = sboOrderMapping;
            }
            else if (ibcOrderMapping != null)
            {
                refOrderMapping = ibcOrderMapping;
            }

            var choice = refOrderMapping.Choice;

            var betType = BetType.None;
            switch (marketType)
            {
                case MarketType.HtHdp:
                case MarketType.FtHdp:
                    if (choice.Equals(refOrderMapping.HomeTeam))
                    {
                        betType = BetType.Home;
                    }
                    else if (choice.Equals(refOrderMapping.AwayTeam))
                    {
                        betType = BetType.Away;
                    }
                    break;

                case MarketType.HtOu:
                case MarketType.FtOu:
                    if (choice.Equals("over", StringComparison.OrdinalIgnoreCase))
                    {
                        betType = BetType.Over;
                    }
                    else if (choice.Equals("under", StringComparison.OrdinalIgnoreCase))
                    {
                        betType = BetType.Under;
                    }
                    break;
            }
            return betType;
        }

        protected TaskCacheDto ConvertToTaskCacheModel(Order order, TasksModel task, Matches refMatch, PlaceOrderCommand req)
        {
            var taskCacheModel = new TaskCacheDto
            {
                Id = order.TaskId,
                MatchMapId = order.MatchMapId,
                MarketType = order.MarketType,
                BetType = order.BetType,
                Status = OrderTaskStatus.Ship,
                CreatedTime = order.CreatedTime,
                CreatedBy = order.CreatedBy,
                Notified = false,
                Seq = task.Seq,
                GameTime = refMatch.Time,
                Stake = req.Stake
            };

            var cacheOrderLines = UkOrderLinesPriceConvert(req.OrderLines);
            taskCacheModel.OrderLines = cacheOrderLines;

            return taskCacheModel;
        }

        // 當 request 的 OrderLines 要寫到 redis cache 裡面時, price 要 -1
        private List<AqsOrderLine> UkOrderLinesPriceConvert(List<AqsOrderLine> ukOrderLines)
        {
            var fixedOrderLines = new List<AqsOrderLine>(ukOrderLines);
            foreach (var orderLine in fixedOrderLines)
            {
                orderLine.Price--;
            }
            return fixedOrderLines;
        }
    }
}