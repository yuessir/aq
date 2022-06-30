using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Application.Orders.V2.PlaceOrder;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Configs;
using AQS.OrderProject.Domain.Customers;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Exceptions;
using AQS.OrderProject.Domain.Reponses;
using AQS.OrderProject.Domain.SharedData;
using Autofac;
using Microsoft.Extensions.Options;
using Navyblue.BaseLibrary;
using Serilog;
using TFA.AQS.Order.Application;

namespace AQS.OrderProject.Application.Orders.V3.PrepareOrder
{
    public class PrepareOrderCommandHandler : ICommandHandler<PrepareOrderCommand, AqsResponse>
    {
        private readonly WebsiteConfig _webSiteConfig;
        private readonly IOrdersRepository _ordersRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IRedisRepository _redisRepository;
        private readonly IOrderDuplicateChecker _orderDuplicateChecker;
        private readonly ILogger _logger;
        private readonly GameConfig _gameConfig;

        public PrepareOrderCommandHandler(IOrdersRepository ordersRepository,
            ITasksRepository tasksRepository,
            IRedisRepository redisRepository,
            ILogger logger,
            IOptions<WebsiteConfig> webSiteConfig,
            IOptions<GameConfig> gameConfig, IOrderDuplicateChecker orderDuplicateChecker)
        {
            this._webSiteConfig = webSiteConfig.Value;
            this._ordersRepository = ordersRepository;
            this._tasksRepository = tasksRepository;
            _redisRepository = redisRepository;
            this._logger = logger;
            _orderDuplicateChecker = orderDuplicateChecker;
            this._gameConfig = gameConfig.Value;
        }

        /// <summary>
        /// Prepare Order - Confirm need to check duplicate orderId or not, reference: OrderServiceV2 line: 45
        /// 
        /// Yes, it's a one to one mapping this is indicated by the orderId field in the content.
        /// Please noted that as per the document these requests could be made more than once but will always refer to the same order if the orderId is the same.
        /// So you will need to have some form of idempotency handling on your end as you should have had already for V2
        ///
        /// Check order is existed or not by orderId - Added by Tommy on 20200904
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AqsResponse> Handle(PrepareOrderCommand request, CancellationToken cancellationToken)
        {
            var userCompany = _webSiteConfig.Websites.FirstOrDefault(p => p.UserName == request.UserName)?.Name;

            Log.Information("[PrepareOrder] Prepare to process prepare order request, user: <{}>, company: <{}>, orderId: <{}>",
                    request.UserName, userCompany, request.OrderId);

            var agentOrderId = Guid.NewGuid().ToString();

            Log.Information("[PrepareOrder] Prepare to process new prepare order request, user: <{}>, company: <{}>, orderId: <{}>, agentOrderId: <{}>",
                    request.UserName, userCompany, request.OrderId, agentOrderId);

            // Extract order and reference match
            var orderAndRefMatch = await GetOrderAndRefMatchFromPrepareOrder(request.UserName, agentOrderId, request);

            var order = orderAndRefMatch.Item1;
            var refMatch = orderAndRefMatch.Item2;

            var seq = await this._tasksRepository.GetNextSeq(request.UserName, order.CreatedTime);

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
            var tasks = await this._tasksRepository.InsertTask(request.UserName, task.Seq, task.MatchId,
                task.MatchMapId, task.MarketType, task.TaskStatus, task.Notified, task.CreatedDate,
                task.CreatedBy, task.BetType, task.FinishTime, task.FinishBy);

            _logger.Information("OrderId: <{}> insert task done, result: {}", request.OrderId, tasks?.ToJson());

            order.SetTaskId(task.Id);

            await this._ordersRepository.InsertOrder(request.UserName, order);

            Log.Information("OrderId: <{}> insert order done, result: {}", request.OrderId, order.ToJson());
            var taskCacheModel = ConvertToTaskCacheModel(order, refMatch, task.Seq);
            _redisRepository.put(RedisCacheCategory.Task, userCompany, taskCacheModel.Id.ToString(), taskCacheModel);
            _logger.Information("OrderId: <{}> insert task to redis done, result: {}", request.OrderId, taskCacheModel.ToJson());

            //notifyEdsNewOrderComing(request.OrderId, request.UserName, order);

            // 這個 received req time 因為 UK prepare order 沒這欄位, 所以這邊塞 controller 收到時間
            var succeedResp= CreateSucceedResp(request.OrderId.Value.ToGuidString(), request.ReceivedReqTime, agentOrderId);

            Log.Information("[PrepareOrder] OrderId: <{}>, AgentOrderId: <{}>, process prepare order request done, response: {}",
                    request.OrderId, agentOrderId, succeedResp.ToJson());

            return succeedResp;
        }

        private AqsResponse CreateSucceedResp(string orderId, DateTime createdTimeUtc, string agentOrderId)
        {
            AqsResponse succeedResp = new()
            {
                OrderId = orderId,
                CreatedTimeUtc = createdTimeUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                AgentOrderId = agentOrderId,
                Code = (int)HttpStatusCode.OK,
                Message = "OK"
            };

            return succeedResp;
        }

        protected TaskCacheDto ConvertToTaskCacheModel(Order order, Matches refMatch, int seq)
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
                Seq = seq,
                GameTime = refMatch.Time
            };

            return taskCacheModel;
        }

        private async Task<Tuple<Order, Matches>> GetOrderAndRefMatchFromPrepareOrder(string currentUser, string agentOrderId, PrepareOrderCommand request)
        {
            // Process order mappings
            var orderMappings = request.OrderMappings;

            var hgOrderMapping = request.OrderMappings.FirstOrDefault(o => o.Book == AqsBook.Singbet);
            var sboOrderMapping = request.OrderMappings.FirstOrDefault(o => o.Book == AqsBook.Sbobet);
            var ibcOrderMapping = request.OrderMappings.FirstOrDefault(o => o.Book == AqsBook.Ibcbet);

            var refOrderMappingAndMatch = await MatchProvider.ProcessMatch(request.OrderId,
                _gameConfig.Host,
                false,
                hgOrderMapping,
                sboOrderMapping,
                ibcOrderMapping,
                _logger);

            var refMatch = refOrderMappingAndMatch.Item2;

            if (refMatch != null)
            {
                var order = Order.PlacePreparedOrder(currentUser, request.OrderId,
                    orderMappings.ToJson(),
                    null,
                    "v3",
                    refMatch.MatchMapId,
                    OrderStatus.Prepare,
                    0,// Task Id - set after insert task
                    agentOrderId,// Agent Id - our system processing id
                    refMatch.Score,
                    request.HitterId,
                    this._orderDuplicateChecker);

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
        private void AddNoMappingToDB(string userName, string agentOrderId, PrepareOrderCommand request,
            OrderMapping hgOrderMapping, OrderMapping sboOrderMapping, OrderMapping ibcOrderMapping)
        {
            Order.PlaceUkNotMappingOrder(userName, request.OrderId,
                request.ReceivedReqTime.ToGmt0Time(),
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

            throw new DataNotFoundException(request.OrderId.Value.ToGuidString(), request.ReceivedReqTime, agentOrderId, message);
        }

        private StringBuilder CreateErrorMessage(PrepareOrderCommand request, OrderMapping hgOrderMapping,
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
    }
}