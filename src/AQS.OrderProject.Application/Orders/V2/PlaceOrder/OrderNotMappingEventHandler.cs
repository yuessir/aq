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

namespace AQS.OrderProject.Application.Orders.V2.PlaceOrder
{
    public class OrderNotMappingEventHandler : INotificationHandler<OrderNotMappingEvent>
    {
        private readonly IUkNotMappingOrdersRepository ordersRepository;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public OrderNotMappingEventHandler(IUkNotMappingOrdersRepository ordersRepository,
        IHttpClientFactory httpClientFactory,
        ILogger logger)
        {
            this.ordersRepository = ordersRepository;
            this._httpClient = httpClientFactory.CreateClient("TaskEndpoint");
            this._logger = logger;
        }

        public async Task Handle(OrderNotMappingEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var ukNotMappingOrder = UkNotMappingOrder.CreateNew(notification.OrderId,
                    notification.CreatedTimeUtc,
                    notification.RequestJson,
                    notification.HgScheduledKickOffTimeUtc,
                    notification.HgLeagueName,
                    notification.HgHomeTeam,
                    notification.HgAwayTeam,
                    notification.SboScheduledKickOffTimeUtc,
                    notification.SboLeagueName,
                    notification.SboHomeTeam,
                    notification.SboAwayTeam,
                    notification.IbcScheduledKickOffTimeUtc,
                    notification.IbcLeagueName,
                    notification.IbcHomeTeam,
                    notification.IbcAwayTeam);

                await this.ordersRepository.InsertUkNotMappingOrder(notification.UserName, ukNotMappingOrder);

                _logger.Information(LogResource.V2POCH08.FormatWith(notification.OrderId));
            }
            catch (Exception e)
            {
                _logger.Warning(LogResource.V2POCH09.FormatWith(notification.OrderId, e.Message));
            }

            string requestBody = notification.ToJson();
            string url = "/Task/AqsMatchNotFound";

            _logger.Information("[Notify] OrderId: {} prepare to send new order notify to url: {}, body: {}", notification.OrderId.Value, url, requestBody);

            try
            {
                var response = await this._httpClient.PostAsJsonAsync("/Task/AqsMatchNotFound", notification, cancellationToken: cancellationToken);
                response.EnsureSuccessStatusCode();
                _logger.Information("[Notify] OrderId: {} send new order notify to url: {} got response, http-status: {}", notification.OrderId.Value, url, HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                _logger.Error("[Notify] OrderId: {} send new order notify to url: {} failed, error-msg: {}", notification.OrderId.Value, url, e.Message);
            }
        }
    }
}