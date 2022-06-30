namespace AQS.OrderProject.Domain.Customers.Orders;

public interface IOrderDuplicateChecker
{
    bool IsDuplicated(string orderId, string userName);
}