using System;
using System.Collections.Generic;
using AQS.OrderProject.Application.Configuration.Queries;

namespace AQS.OrderProject.Application.Orders.GetCustomerOrders
{
    public class GetCustomerOrdersQuery : IQuery<List<OrderDto>>
    {
        public Guid CustomerId { get; }

        public GetCustomerOrdersQuery(Guid customerId)
        {
            this.CustomerId = customerId;
        }
    }
}