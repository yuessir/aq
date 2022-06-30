using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AQS.OrderProject.Domain.Customers.Orders
{
    public interface IOrdersRepository
    {
        Task InsertOrder(string currentUser, Order order);

        Task<List<Order>> FindAll(string currentUser);

        Task<int> GetTaskIdFromOrder(string currentUser, OrderId orderId);

        Task<Order> GetOrderByOrderId(string currentUser, OrderId orderId);

        Task<bool> OrderExisted(string currentUser, OrderId orderId);

        Task<Order> DeleteOrder(string currentUser, OrderId orderId);

        Task UpdateOrderFields(string currentUser, OrderId orderId, Market market, MarketType marketType, string orderMappings, string orderLines, OrderStatus orderStatus);

        Task<Order> UpdateOrderCreatedBy(string currentUser, OrderId orderId, string createBy);

        Task UpdateOrderUpdateTime(string currentUser, OrderId orderId, DateTime updateTime);

        Task UpdateOrderLine(string currentUser, OrderId orderId, string orderLines);
    }
}