using System;
using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Reponses;

namespace AQS.OrderProject.Application.Orders.V2.CancelOrder
{
    public class CancelOrderCommand : CommandBase<AqsResponse>
    {
        public OrderId OrderId { get; set; }

        public DateTime CreatedTime { get; set; }

        public string HitterId { get; set; }

        public CancelOrderCommand(Guid orderId,
            DateTime createdTime,
            string hitterId)
        {
            this.OrderId = new OrderId(orderId);
            this.CreatedTime = createdTime;
            this.HitterId = hitterId;
        }
    }
}