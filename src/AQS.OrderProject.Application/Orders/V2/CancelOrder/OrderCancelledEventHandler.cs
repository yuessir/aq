using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Customers.Orders.Events;
using MediatR;
using Navyblue.BaseLibrary;
using Serilog;

namespace AQS.OrderProject.Application.Orders.V2.CancelOrder
{
    public class OrderCancelledEventHandler : INotificationHandler<OrderCancelledEvent>
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IRedisRepository _redisRepository;
        private readonly ILogger _logger;

        public OrderCancelledEventHandler(
            IOrdersRepository ordersRepository,
            ITasksRepository tasksRepository,
            IRedisRepository redisRepository,
            ILogger logger)
        {
            this._ordersRepository = ordersRepository;
            this._tasksRepository = tasksRepository;
            this._redisRepository = redisRepository;

            this._logger = logger;
        }

        public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
        {
            Log.Information("[CancelOrder] OrderId: <{}> update order status to DELETED done", notification.OrderId.Value);

            var taskId = await this._ordersRepository.GetTaskIdFromOrder(notification.CurrentUser, notification.OrderId);

            await this._tasksRepository.UpdateTaskToCancel(notification.CurrentUser, taskId, OrderTaskStatus.CustomerCanceled, new DateTime(), notification.HitterId);

            Log.Information(
                "[CancelOrder] OrderId: <{}> update tasks status to CUSOMER_CANCELED (7) and BetType to -1 done, taskId: <{}>, hitterId: <{}>",
                notification.OrderId.Value, taskId, notification.HitterId);

            await RemoveTaskFromRedis(notification.OrderId.Value.ToGuidString(), taskId, notification.CompanyName);

            Log.Information("[CancelOrder] OrderId: <{}> cancel order with hitter: <{}> done", notification.OrderId.Value, notification.HitterId);
        }

        private async Task RemoveTaskFromRedis(string orderId, int taskId, string companyName)
        {
            string removedKey = await this._redisRepository.remove(RedisCacheCategory.Task, companyName, taskId.ToString());

            if (!string.IsNullOrWhiteSpace(removedKey))
            {
                _logger.Information(
                    "[CancelOrder] OrderId: <{}> remove task from redis done, company: <{}>, taskId: <{}>, removedKey: <{}>",
                    orderId, companyName, taskId, removedKey);
            }
            else
            {
                _logger.Warning("[CancelOrder] OrderId: <{}> cannot find mapping task at redis, company: <{}>, taskId: <{}>",
                    orderId, companyName, taskId);
            }
        }
    }
}