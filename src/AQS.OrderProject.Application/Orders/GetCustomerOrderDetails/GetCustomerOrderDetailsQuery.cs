using System;
using AQS.OrderProject.Application.Configuration.Queries;

namespace AQS.OrderProject.Application.Orders.GetCustomerOrderDetails
{
    public class GetCustomerOrderDetailsQuery : IQuery<OrderDetailsDto>
    {
        public Guid OrderId { get; }

        public GetCustomerOrderDetailsQuery(Guid orderId)
        {
            this.OrderId = orderId;
        }
    }
}