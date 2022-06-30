using System;
using AQS.OrderProject.Domain.SeedWork;

namespace AQS.OrderProject.Domain.Customers.Orders.Events
{
    public class OrderPlacedEvent : DomainEventBase
    {
        public OrderId OrderId { get; init; }

        public DateTime CreatedTime { get; init; }

        public Market Market { get; init; }

        public MatchType MatchType { get; init; }

        public MarketType MarketType { get; init; }

        public BetType BetType { get; init; }

        public string CreatedBy { get; init; }

        public string OrderLines { get; init; }

        public string OrderVersion { get; init; }

        public int? MatchMapId { get; init; }

        public OrderStatus OrderStatus { get; init; }

        public int TaskId { get; init; }

        public string AgentId { get; init; }

        public string Score { get; init; }

        public DateTime CreatedTimeUtc { get; init; }

        public AqsMarket Phase { get; init; }

        public string ScoreType { get; init; }

        public AqsStage Stage { get; init; }

        public string HitterId { get; init; }

        public string OrderMappings { get; init; }

        public DateTime UpdateTime { get; init; }

        public OrderPlacedEvent(OrderId orderId,
            Market market,
            MatchType matchType,
            MarketType marketType,
            BetType betType,
            string createdBy,
            string orderLines,
            string orderVersion,
            int? matchMapId,
            OrderStatus orderStatus,
            int taskId,
            string agentId,
            string score,
            string orderMappings,
            DateTime createdTime,
            DateTime updateTime)
        {
            this.OrderId = orderId;
            this.CreatedTime = createdTime;
            this.Market = market;
            this.MatchType = matchType;
            this.MarketType = marketType;
            this.BetType = betType;
            this.CreatedBy = createdBy;
            this.OrderLines = orderLines;
            this.OrderVersion = orderVersion;
            this.MatchMapId = matchMapId;
            this.OrderStatus = orderStatus;
            this.TaskId = taskId;
            this.AgentId = agentId;
            this.Score = score;
            this.OrderMappings = orderMappings;
            this.UpdateTime = updateTime;
        }
    }
}