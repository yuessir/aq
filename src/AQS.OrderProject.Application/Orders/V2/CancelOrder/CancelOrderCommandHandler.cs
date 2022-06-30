using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Domain.Configs;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Exceptions;
using AQS.OrderProject.Domain.Reponses;
using Microsoft.Extensions.Options;
using Navyblue.BaseLibrary;
using Serilog;

namespace AQS.OrderProject.Application.Orders.V2.CancelOrder
{
    public class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, AqsResponse>
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly ILogger _logger;
        private readonly WebsiteConfig _websiteConfig;

        public CancelOrderCommandHandler(
            IOrdersRepository ordersRepository, 
            ILogger logger,
            IOptions<WebsiteConfig> websiteConfig)
        {
            this._ordersRepository = ordersRepository;

            this._logger = logger;
            this._websiteConfig = websiteConfig.Value;
        }

        public async Task<AqsResponse> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
        {
            var agentOrderId = Guid.NewGuid().ToString();
            string companyName = _websiteConfig.GetCompanyName(command.UserName);
            _logger.Information("[CancelOrder] Prepare to process cancel order request, user: <{}>, company: <{}>, orderId: <{}>, agentOrderId: <{}>", command.UserName, companyName, command.OrderId, agentOrderId);

            var order = await this._ordersRepository.DeleteOrder(command.UserName, command.OrderId);
            if (order == null)
            {
                _logger.Warning("[CancelOrder] Order not found with orderId: <{}>", command.OrderId);
                var message = "Order not found";
                throw new DataNotFoundException(command.OrderId.Value.ToGuidString(), command.CreatedTime, null, message);
            }

            order.Cancel(command.UserName, command.OrderId, command.HitterId, companyName);
            var succeedResp = CreateSucceedResp(command.OrderId, command.CreatedTime, agentOrderId);
            _logger.Information("[CancelOrder] Process cancel order done, response: {}", succeedResp.ToJson());

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
    }
}