using AQS.OrderProject.Domain.SeedWork;

namespace AQS.OrderProject.Domain.Customers.Orders.Events
{
    public class OrderCancelledEvent : DomainEventBase
    {
        public OrderId OrderId { get; }

        public string CurrentUser { get; }

        public string HitterId { get; }

        public string CompanyName { get; }

        public OrderCancelledEvent(OrderId orderId,
            string currentUser,
            string hitterId,
            string companyName)
        {
            this.OrderId = orderId;
            this.CurrentUser = currentUser;
            this.HitterId = hitterId;
            this.CompanyName = companyName;
        }
    }
}