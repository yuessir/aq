using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AQS.OrderProject.Domain.Customers.Orders.Events;
using MediatR;
using Navyblue.BaseLibrary;
using Serilog;

namespace AQS.OrderProject.Application.Orders.V2.PlaceOrder
{
    public class OrderPlacedDomainEventHandler : INotificationHandler<OrderPlacedEvent>
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public OrderPlacedDomainEventHandler(IHttpClientFactory httpClientFactory,
            ILogger logger)
        {
            this._httpClient = httpClientFactory.CreateClient("TaskEndpoint");
            this._logger = logger;
        }

        public async Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
        {
            string requestBody = notification.ToJson();
            string url = "/Task/NewOrder";

            _logger.Information("[Notify] OrderId: <{}> prepare to send new order notify to url: {}, body: {}", notification.OrderId, url, requestBody);

            try
            {

                var response = await this._httpClient.PostAsJsonAsync("/Task/NewOrder", notification, cancellationToken: cancellationToken);

                _logger.Information("[Notify] OrderId: <{}> send new order notify to url: {} got response, http-status: {}", notification.OrderId, url, response.StatusCode);
            }
            catch (Exception e)
            {
                _logger.Error("[Notify] OrderId: <{}> send new order notify to url: {} failed, error-msg: {}", notification.OrderId, url, e.Message);
            }
        }
    }
}