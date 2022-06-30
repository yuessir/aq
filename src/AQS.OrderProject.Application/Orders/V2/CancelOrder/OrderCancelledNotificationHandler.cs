using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AQS.OrderProject.Domain.Customers.Orders.Events;
using MediatR;
using Navyblue.BaseLibrary;
using Serilog;

namespace AQS.OrderProject.Application.Orders.V2.CancelOrder
{
    public class OrderPlacedNotificationHandler : INotificationHandler<OrderCancelledNotification>
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public OrderPlacedNotificationHandler(IHttpClientFactory httpClientFactory,
            ILogger logger)
        {
            this._httpClient = httpClientFactory.CreateClient("TaskEndpoint");
            this._logger = logger;
        }

        public async Task Handle(OrderCancelledNotification notification, CancellationToken cancellationToken)
        {
            string url = $"/Task/CancelOrder?orderId={notification.OrderId}";

            _logger.Information("[Notify] OrderId: <{}> prepare to send cancel order notify to url: {}", notification.OrderId, url);

            try
            {

                var response = await this._httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                _logger.Information("[Notify] OrderId: <{}> send cancel order notify to url: {}, http-status: {}", notification.OrderId, url, HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                _logger.Error("[Notify] OrderId: <{}> send cancel order notify to url: {} failed, error-msg: {}", notification.OrderId, url, e.Message);
            }
        }
    }
}