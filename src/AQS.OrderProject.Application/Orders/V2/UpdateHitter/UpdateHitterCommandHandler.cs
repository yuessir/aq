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

namespace AQS.OrderProject.Application.Orders.V2.UpdateHitter
{
    public class UpdateHitterCommandHandler : ICommandHandler<UpdateHitterCommand, AqsResponse>
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly WebsiteConfig _websiteConfig;

        public UpdateHitterCommandHandler(IOrdersRepository ordersRepository,
            IOptions<WebsiteConfig> webSiteConfig)
        {
            this._ordersRepository = ordersRepository;
            this._websiteConfig = webSiteConfig.Value;
        }

        public async Task<AqsResponse> Handle(UpdateHitterCommand command, CancellationToken cancellationToken)
        {
            var agentOrderId = Guid.NewGuid().ToGuidString();

            string userCompany = _websiteConfig.GetCompanyName(command.UserName);

            Log.Information(
                "[UpdateHitter] Prepare to process update hitter request, user: <{}>, company: <{}>, orderId: <{}>, agentOrderId: <{}>",
                command.UserName, userCompany, command.OrderId, agentOrderId);

            var order = await this._ordersRepository.UpdateOrderCreatedBy(command.UserName, new OrderId(command.OrderId.Value), command.HitterId);
            if (order == null)
            {
                Log.Warning("[UpdateHitter] Order not found with orderId: <{}>", command.OrderId);

                var message = "Order not found";

                throw new DataNotFoundException(command.OrderId.Value.ToGuidString(), command.CreatedTime, null, message);
            }

            Log.Information("[UpdateHitter] OrderId: <{}> update order hitter to {}", command.OrderId, command.HitterId);

            var succeedResp
                = CreateSucceedResp(command.OrderId.Value.ToGuidString(), command.CreatedTime, agentOrderId);

            Log.Information("[UpdateHitter] Process update hitter done, response: {}", succeedResp.ToJson());

            return succeedResp;
        }

        private AqsResponse CreateSucceedResp(string orderId, DateTime createdTime, string agentOrderId)
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
    }
}