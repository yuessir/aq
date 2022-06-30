using System;
using System.Collections.Generic;
using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Reponses;

namespace AQS.OrderProject.Application.Orders.V3.UpdateOrder
{
    public class UpdateOrderCommand : CommandBase<AqsResponse>
    {
        public OrderId OrderId { get; init; }

        public DateTime CreatedTimeUtc { get; init; }

        public string HitterId { get; init; }

        public long Stake { get; init; }

        public List<AqsOrderLine> OrderLines { get; init; }

        public UpdateOrderCommand(Guid orderId,
            DateTime createdTimeUtc,
            string hitterId,
            long stake,
            List<AqsOrderLine> orderLines)
        {
            this.OrderId = new OrderId(orderId);
            this.CreatedTimeUtc = createdTimeUtc;
            this.HitterId = hitterId;
            this.Stake = stake;
            this.OrderLines = orderLines;
        }
    }
}