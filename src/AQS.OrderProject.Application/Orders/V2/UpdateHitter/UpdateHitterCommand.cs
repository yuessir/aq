using System;
using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Reponses;

namespace AQS.OrderProject.Application.Orders.V2.UpdateHitter
{
    public class UpdateHitterCommand : CommandBase<AqsResponse>
    {
        public OrderId OrderId { get; init; }

        public DateTime CreatedTime { get; init; }

        public string HitterId { get; init; }

        public UpdateHitterCommand(Guid orderId,
            DateTime createdTime,
            string hitterId)
        {
            this.OrderId = new OrderId(orderId); 
            this.CreatedTime=createdTime;
            this.HitterId = hitterId;
        }
    }
}