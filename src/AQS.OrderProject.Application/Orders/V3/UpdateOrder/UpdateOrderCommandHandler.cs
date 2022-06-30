using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Configs;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Exceptions;
using AQS.OrderProject.Domain.Reponses;
using Microsoft.Extensions.Options;
using Navyblue.BaseLibrary;
using Serilog;

namespace AQS.OrderProject.Application.Orders.V3.UpdateOrder
{
    public class UpdateOrderCommandHandler : ICommandHandler<UpdateOrderCommand, AqsResponse>
    {
        private readonly WebsiteConfig _webSiteConfig;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IRedisRepository _redisRepository;
        private readonly ILogger _logger;

        public UpdateOrderCommandHandler(IOrdersRepository ordersRepository,
            IRedisRepository redisRepository,
            ILogger logger, 
            IOptions<WebsiteConfig> webSiteConfig)
        {
            this._webSiteConfig = webSiteConfig.Value;
            this._ordersRepository = ordersRepository;
            this._logger = logger;
            _redisRepository = redisRepository;
        }

        public async Task<AqsResponse> Handle(UpdateOrderCommand req, CancellationToken cancellationToken)
        {
            var userCompany = _webSiteConfig.Websites.FirstOrDefault(p => p.UserName == req.UserName)?.Name;

            Log.Information("[UpdateOrder] OrderId: <{}> prepare to process update order request from user: <{}>, company: <{}>",
                    req.OrderId, req.UserName, userCompany);

            String agentOrderId = null;

            // Check Order is existed or not by orderId
            var orderInDb = await this._ordersRepository.GetOrderByOrderId(req.UserName, req.OrderId);
            if (orderInDb != null)
            {
                agentOrderId = orderInDb.AgentId;

                Log.Information("[UpdateOrder] OrderId: <{}> find mapping order in database, agentOrderId: <{}>",
                        req.OrderId, orderInDb.AgentId);

                // Update table: orders, field: UpdateTime
                DateTime updateTime = DateTime.UtcNow;
                await this._ordersRepository.UpdateOrderUpdateTime(req.UserName, req.OrderId, updateTime);

                Log.Information("OrderId: <{}> update order updateTime to <{}> done", req.OrderId, updateTime);

                // Set OrderLines
                await this._ordersRepository.UpdateOrderLine(req.UserName, req.OrderId, req.OrderLines.ToJson());

                Log.Information("OrderId: <{}> update order lines to <{}> done", req.OrderId, req.OrderLines.ToJson());

                // Update redis: task, fields: stake, orderLines
                var taskFromRedis = await _redisRepository.get<TaskCacheDto>(RedisCacheCategory.Task, userCompany, orderInDb.TaskId.ToString());
                _logger.Information("OrderId: <{}> get task from redis succeed, taskId: <{}>, content: {}", req.OrderId, orderInDb.TaskId,
                    taskFromRedis);
                if (taskFromRedis != null)
                {
                    // Set Stake
                    taskFromRedis.Stake = req.Stake;

                    // Set OrderLines
                    var cacheOrderLines = UkOrderLinesPriceConvert(req.OrderLines);
                    taskFromRedis.OrderLines = cacheOrderLines;

                    // Update redis cache
                    var taskId = taskFromRedis.Id;
                    _redisRepository.put(RedisCacheCategory.Task, userCompany, taskId.ToString(), taskFromRedis);
                    _logger.Information("OrderId: <{}> update task at redis succeed, taskId: <{}>, content: {}", req.OrderId, taskId,
                        taskFromRedis.ToJson());

                    // Notify eds new order update
                    orderInDb = await this._ordersRepository.GetOrderByOrderId(req.UserName, req.OrderId);
                    //notifyEdsUpdateOrder(req.OrderId, req.UserName, orderInDB);
                }
                else
                {
                    Log.Warning("[UpdateOrder] OrderId: <{}> cannot find mapping task from redis with taskId: <{}>, please check...",
                            req.OrderId, orderInDb.TaskId);

                    var message = "Order not found";

                    throw new DataNotFoundException(req.OrderId.Value.ToGuidString(), req.CreatedTimeUtc, null, message);
                }
            }
            // Cannot find mapping order from table: orders
            else
            {
                Log.Warning("[UpdateOrder] OrderId: <{}> cannot find mapping order, please check...", req.OrderId);

                var message = "Order not found";

                throw new DataNotFoundException(req.OrderId.Value.ToGuidString(), req.CreatedTimeUtc, null, message);
            }

            var succeedResp
                    = CreateSucceedResp(req.OrderId.Value, req.CreatedTimeUtc, agentOrderId);

            Log.Information("[UpdateOrder] OrderId: <{}> process update order request done, response: {}",
                    req.OrderId, succeedResp.ToJson());

            return succeedResp;
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

        protected AqsResponse CreateSucceedResp(Guid orderId, DateTime createdTimeUtc, string agentOrderId)
        {
            AqsResponse succeedResp = new()
            {
                OrderId = orderId.ToGuidString(),
                CreatedTimeUtc = createdTimeUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                AgentOrderId = agentOrderId,
                Code = (int)HttpStatusCode.OK,
                Message = "OK"
            };

            return succeedResp;
        }
    }
}