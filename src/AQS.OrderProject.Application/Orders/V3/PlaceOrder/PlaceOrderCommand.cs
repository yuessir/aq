using System;
using System.Collections.Generic;
using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Reponses;
using Navyblue.BaseLibrary;
using TFA.AQS.Order.Domain.Requests.V3;

namespace AQS.OrderProject.Application.Orders.V3.PlaceOrder
{
    public class PlaceOrderCommand : CommandBase<AqsResponse>
    {
        public OrderId OrderId { get; set; }

        public DateTime CreatedTime { get; set; }

        public AqsMarket Phase { get; set; }

        public string ScoreType { get; set; }

        public AqsStage Stage { get; set; }

        public AqsMarketType MarketType { get; set; }

        public string HitterId { get; set; }

        public long Stake { get; set; }

        public List<OrderMapping> OrderMappings { get; set; }

        public AqsTranslation Translation { get; set; }

        public List<AqsOrderLine> OrderLines { get; set; }

        /// <summary>
        /// 用來讓 UK 知道我們處理這筆的系統 id
        /// </summary>
        public string AgentOrderId { get; init; }

        public PlaceOrderCommand(Guid orderId,
            AqsMarket phase,
            string scoreType,
            AqsStage stage,
            AqsMarketType marketType,
            string hitterId,
            long stake,
            List<OrderMapping> orderMappings,
            AqsTranslation translation,
            List<AqsOrderLine> orderLines)
        {
            this.OrderId = new OrderId(orderId);
            this.CreatedTime = CreatedTime;
            this.Phase = phase;
            this.ScoreType = scoreType;
            this.Stage = stage;
            this.MarketType = marketType;
            this.HitterId = hitterId;
            this.Stake = stake;
            this.OrderMappings = orderMappings;
            this.Translation = translation;
            this.OrderLines = orderLines;
            this.AgentOrderId = AgentOrderId;

            this.AgentOrderId = Guid.NewGuid().ToGuidString();
        }
    }
}