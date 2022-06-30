using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AQS.OrderProject.Domain.Configs;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Infrastructure.Domain.SharedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AQS.OrderProject.Infrastructure.Domain.Orders
{
    public class OrdersRepository : BaseRepository, IOrdersRepository
    {
        public OrdersRepository(IOptions<WebsiteConfig> websiteConfig) : base(websiteConfig)
        {
        }

        public async Task InsertOrder(string currentUser, Order order)
        {
            await using var context = this.GetContext(currentUser);
            await context.Orders.AddAsync(order);
        }

        public async Task<List<Order>> FindAll(string currentUser)
        {
            await using var context = this.GetContext(currentUser);
            return await context.Orders.OrderByDescending(p => p.CreatedTime).Take(10).ToListAsync();
        }

        public async Task<int> GetTaskIdFromOrder(string currentUser, OrderId orderId)
        {
            await using var context = this.GetContext(currentUser);
            var result = await context.Orders.FirstOrDefaultAsync(p => p.OrderId == orderId);

            return result?.TaskId ?? 0;
        }

        public async Task<Order> GetOrderByOrderId(string currentUser, OrderId orderId)
        {
            await using var context = this.GetContext(currentUser);
            return await context.Orders.FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<bool> OrderExisted(string currentUser, OrderId orderId)
        {
            await using var context = this.GetContext(currentUser);
            return await context.Orders.AnyAsync(p => p.OrderId == orderId);
        }

        public async Task<Order> DeleteOrder(string currentUser, OrderId orderId)
        {
            await using var context = this.GetContext(currentUser);
            var order = await context.Orders.FirstOrDefaultAsync(p => p.OrderId == orderId);
            if (order != null)
            {
                order.Change(order.Market, order.MarketType, order.OrderMappings, order.OrderLines, OrderStatus.Deleted, order.CreatedBy);
                context.Orders.Update(order);

                return order;
            }

            return null;
        }

        public async Task UpdateOrderFields(string currentUser, OrderId orderId, Market market, MarketType marketType, string orderMappings, string orderLines, OrderStatus orderStatus)
        {
            await using var context = this.GetContext(currentUser);
            var order = await context.Orders.FirstOrDefaultAsync(p => p.OrderId == orderId);
            if (order != null)
            {
                order.Change(market, marketType, orderMappings, orderLines, orderStatus, order.CreatedBy);
                context.Orders.Update(order);
            }
        }

        public async Task<Order> UpdateOrderCreatedBy(string currentUser, OrderId orderId, string createBy)
        {
            await using var context = this.GetContext(currentUser);
            var order = await context.Orders.FirstOrDefaultAsync(p => p.OrderId == orderId);
            if (order != null)
            {
                order.Change(order.Market, order.MarketType, order.OrderMappings, order.OrderLines, OrderStatus.Deleted, createBy);
                context.Orders.Update(order);

                return order;
            }

            return null;
        }

        public async Task UpdateOrderUpdateTime(string currentUser, OrderId orderId, DateTime updateTime)
        {
            await using var context = this.GetContext(currentUser);
            var order = await context.Orders.FirstOrDefaultAsync(p => p.OrderId == orderId);
            if (order != null)
            {
                order.Change(order.Market, order.MarketType, order.OrderMappings, order.OrderLines, OrderStatus.Deleted, order.CreatedBy, updateTime);
                context.Orders.Update(order);
            }
        }

        public async Task UpdateOrderLine(string currentUser, OrderId orderId, string orderLines)
        {
            await using var context = this.GetContext(currentUser);
            var order = await context.Orders.FirstOrDefaultAsync(p => p.OrderId == orderId);
            if (order != null)
            {
                order.Change(order.Market, order.MarketType, order.OrderMappings, orderLines, OrderStatus.Deleted, order.CreatedBy);
                context.Orders.Update(order);
            }
        }
    }
}