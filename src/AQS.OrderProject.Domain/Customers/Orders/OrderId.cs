using System;
using AQS.OrderProject.Domain.SeedWork;

namespace AQS.OrderProject.Domain.Customers.Orders
{
    public class OrderId : TypedIdValueBase
    {
        public OrderId(Guid value) : base(value)
        {
        }
    }
}