using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Configs;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Exceptions;
using AQS.OrderProject.Domain.Reponses;
using AQS.OrderProject.Domain.SharedData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Navyblue.BaseLibrary;
using Serilog;
using TFA.AQS.Order.Application;
using OrderTaskStatus = AQS.OrderProject.Domain.OrderTaskStatus;

namespace AQS.OrderProject.Application.Orders.V2.PlaceOrder
{
    public class PlaceCustomerOrderCommandHandler : ICommandHandler<PlaceCustomerOrderCommand, AqsResponse>
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IRedisRepository _redisRepository;
        private readonly IOrderDuplicateChecker _orderDuplicateChecker;
        private readonly ILogger _logger;
        private readonly WebsiteConfig _webSiteConfig =new WebsiteConfig();
        private readonly GameConfig _gameConfig;

        public PlaceCustomerOrderCommandHandler(
            IOrdersRepository ordersRepository,
            IRedisRepository redisRepository,
            IOrderDuplicateChecker orderDuplicateChecker,
            ILogger logger,
            IOptions<WebsiteConfig> webSiteConfig,
            IOptions<GameConfig> gameConfig)
        {
            this._redisRepository = redisRepository;
            this._ordersRepository = ordersRepository;
            this._orderDuplicateChecker = orderDuplicateChecker;
            this._logger = logger;
            this._webSiteConfig = webSiteConfig.Value;
            this._gameConfig = gameConfig.Value;
        }

        /// <summary>
        /// 当前Order中有TaskId，相当于每个订单需要有一个task与之匹配，如果一个订单有多个Task的时候，这里的设计就无法满足了
        /// 建议： Task应该依赖于Order，原因是针对Order可以有多种类型的Task，同时Order之Task的生成也应该依赖于事件
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AqsResponse> Handle(PlaceCustomerOrderCommand command, CancellationToken cancellationToken)
        {
            string companyName = _webSiteConfig.GetCompanyName(command.UserName);
            
            _logger.Information(LogResource.V2POCH01.FormatWith(command.UserName, companyName, command.OrderId, command.AgentOrderId));

            var orderExisted = await this._ordersRepository.OrderExisted(command.UserName, command.OrderId);
            if (orderExisted)
            {
                string message = LogResource.V2POCH02.FormatWith(command.OrderId, command.AgentOrderId);
                _logger.Warning(message);
                throw new Exception(message);
            }

            var orderAndRefMatch = await this.GetOrderAndRefMatchFromReq(command.UserName, command.AgentOrderId, command);

            var order = orderAndRefMatch.Item1;
            var match = orderAndRefMatch.Item2;

            var taskDuplicated = await OrderTaskProvider.CheckTaskDuplicated(command.UserName, order.MatchMapId, order.MarketType);
            if (taskDuplicated)
            {
                var errorMessage = LogResource.V2POCH04.FormatWith(command.OrderId, order.MatchMapId, order.MarketType, command.AgentOrderId);
                _logger.Warning(errorMessage);
                throw new Exception(errorMessage);
            }

            OrderTask tasks = await OrderTaskProvider.InsertTask(command.UserName, order, _logger);

            await this._ordersRepository.InsertOrder(command.UserName, order);

            _logger.Information(LogResource.V2POCH05.FormatWith(order.OrderId, order.ToJson()));

            var taskCacheModel = ConvertToTaskCacheModel(order, match, tasks.Seq);
            _redisRepository.put(RedisCacheCategory.Task, companyName, taskCacheModel.Id.ToString(), taskCacheModel);
            _logger.Information(LogResource.V2POCH06.FormatWith(order.OrderId, taskCacheModel.ToJson()));

            var succeedResp = this.CreateSucceedResp(command.OrderId, command.CreatedTime, command.AgentOrderId);

            _logger.Information(LogResource.V2POCH06.FormatWith(command.OrderId, succeedResp.ToJson()));

            return succeedResp;
        }

        private AqsResponse CreateSucceedResp(OrderId orderId, DateTime createdTime, string agentOrderId)
        {
            AqsResponse succeedResp = new()
            {
                OrderId = orderId.Value.ToGuidString(),
                CreatedTimeUtc = createdTime.ToString("yyyy-MM-dd HH:mm:ss"),
                AgentOrderId = agentOrderId,
                Code = (int)HttpStatusCode.OK,
                Message = "OK"
            };

            return succeedResp;
        }

        protected TaskCacheDto ConvertToTaskCacheModel(Order order, Matches match, int seq)
        {
            TaskCacheDto taskCacheModel = new()
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
                GameTime = match.Time
            };

            return taskCacheModel;
        }

        private async Task<Tuple<Order, Matches>> GetOrderAndRefMatchFromReq(string currentUser, string agentOrderId, PlaceCustomerOrderCommand command)
        {
            var hgOrderMapping = command.OrderMappings.FirstOrDefault(o => o.Book == AqsBook.Singbet);
            var sboOrderMapping = command.OrderMappings.FirstOrDefault(o => o.Book == AqsBook.Sbobet);
            var ibcOrderMapping = command.OrderMappings.FirstOrDefault(o => o.Book == AqsBook.Ibcbet);

            var tuple = await MatchProvider.ProcessMatch(command.OrderId,
                _gameConfig.Host,
                command.Phase == AqsMarket.InRunning,
                hgOrderMapping,
                sboOrderMapping,
                ibcOrderMapping,
                _logger);

            var refOrderMapping = tuple.Item1;
            var refMatch = tuple.Item2;

            if (refMatch != null)
            {
                var order = Order.PlaceOrder(currentUser,
                    command.OrderId,
                    MatchType.Normal, // FIXME EDS order 的 match type 可能要根據 UK 來的 Score Type 來決定
                    command.OrderMappings.ToJson(),
                    null,
                    "v2",
                    refMatch.MatchMapId,
                    OrderStatus.Running,
                    0,
                    agentOrderId,
                    refMatch.Score,
                    command.HitterId,
                    command.CreatedTime.ToGmt0Time(),
                    command.CreatedTime.ToGmt0Time(),
                    command.Phase,
                    command.Stage,
                    command.MarketType,
                    refOrderMapping.Choice,
                    refOrderMapping.HomeTeam,
                    refOrderMapping.AwayTeam,
                    _orderDuplicateChecker
                );

                return new Tuple<Order, Matches>(order, refMatch);
            }

            AddNoMappingToDB(currentUser, agentOrderId, command, hgOrderMapping, sboOrderMapping, ibcOrderMapping);

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
        private void AddNoMappingToDB(string userName, string agentOrderId, PlaceCustomerOrderCommand request,
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

        private StringBuilder CreateErrorMessage(PlaceCustomerOrderCommand request, OrderMapping hgOrderMapping,
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