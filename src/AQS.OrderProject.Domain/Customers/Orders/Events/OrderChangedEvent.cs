using AQS.OrderProject.Domain.SeedWork;

namespace AQS.OrderProject.Domain.Customers.Orders.Events
{
    public class OrderChangedEvent : DomainEventBase
    {
        public OrderId OrderId { get; }

        public OrderChangedEvent(OrderId orderId)
        {
            this.OrderId = orderId;
        }
    }
}