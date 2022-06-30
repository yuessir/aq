using System;
using System.Collections.Generic;
using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Reponses;
using TFA.AQS.Order.Domain.Requests.V3;

namespace AQS.OrderProject.Application.Orders.V3.PrepareOrder
{
    public class PrepareOrderCommand : CommandBase<AqsResponse>
    {
        public OrderId OrderId { get; init; }

        public String HitterId { get; init; }

        public List<OrderMapping> OrderMappings { get; init; }

        public AqsTranslation Translation { get; init; }

        // 收到 request 時塞入
        public DateTime ReceivedReqTime { get; init; }

        public PrepareOrderCommand(Guid orderId,
            string hitterId,
            List<OrderMapping> orderMappings,
            AqsTranslation translation,
            DateTime receivedReqTime)
        {
            this.OrderId = new OrderId(orderId);
            this.HitterId = hitterId;
            this.OrderMappings = orderMappings;
            this.Translation = translation;
            this.ReceivedReqTime = receivedReqTime;
        }
    }
}