using System;
using System.Collections.Generic;
using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Reponses;
using Navyblue.BaseLibrary;

namespace AQS.OrderProject.Application.Orders.V2.PlaceOrder
{
    public class PlaceCustomerOrderCommand : CommandBase<AqsResponse>
    {
        public OrderId OrderId { get; init; }

        public DateTime CreatedTime { get; init; }

        public AqsMarket Phase { get; init; }

        public string ScoreType { get; init; }

        public AqsStage Stage { get; init; }

        public AqsMarketType MarketType { get; init; }

        public string HitterId { get; init; }

        public List<OrderMapping> OrderMappings { get; init; }

        /// <summary>
        /// 用來讓 UK 知道我們處理這筆的系統 id
        /// </summary>
        public string AgentOrderId { get; init; }

        public PlaceCustomerOrderCommand(string userName, string orderId,
            AqsMarketType marketType,
            List<OrderMapping> orderMappings,
            string scoreType,
            AqsStage stage,
            string hitterId)
        {
            this.OrderId = new OrderId(orderId.ToGuid());
            this.MarketType = marketType;
            this.OrderMappings = orderMappings;
            this.CreatedTime = DateTime.UtcNow;
            this.ScoreType = scoreType;
            this.Stage = stage;
            this.HitterId = hitterId;
            this.UserName = userName;
            this.AgentOrderId = Guid.NewGuid().ToGuidString();
        }
    }
}