using AQS.OrderProject.Application.Configuration.DomainEvents;
using AQS.OrderProject.Domain.Customers;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Customers.Orders.Events;
using Newtonsoft.Json;

namespace AQS.OrderProject.Application.Orders.V2.CancelOrder
{
    public class OrderCancelledNotification : DomainNotificationBase<OrderCancelledEvent>
    {
        public OrderId OrderId { get; }

        public OrderCancelledNotification(OrderCancelledEvent domainEvent) : base(domainEvent)
        {
            this.OrderId = domainEvent.OrderId;
        }

        [JsonConstructor]
        public OrderCancelledNotification(OrderId orderId) : base(null)
        {
            this.OrderId = orderId;
        }
    }
}