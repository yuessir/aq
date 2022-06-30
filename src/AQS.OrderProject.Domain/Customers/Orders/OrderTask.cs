using System;
using AQS.OrderProject.Domain.SeedWork;

namespace AQS.OrderProject.Domain.Customers.Orders
{
    public class OrderTask : Entity, IAggregateRoot
    {
        public int Seq { get; private set; }

        public int MatchId { get; private set; }

        public int? MatchMapId { get; private set; }

        public MarketType MarketType { get; private set; }

        public OrderTaskStatus TaskStatus { get; private set; }

        public bool Notified { get; private set; }

        public DateTime CreatedDate { get; private set; }

        public string CreatedBy { get; private set; }

        public BetType BetType { get; private set; }

        public DateTime FinishTime { get; private set; }

        public string FinishBy { get; private set; }

        private OrderTask()
        {

        }

        public OrderTask(int seq,
            int matchId,
            int? matchMapId,
            MarketType marketType,
            OrderTaskStatus taskStatus,
            bool notified,
            DateTime createdDate,
            string createdBy,
            BetType betType,
            DateTime finishTime,
            string finishBy)
        {
            this.Seq = seq;
            this.MatchId = matchId;
            this.MatchMapId = matchMapId;
            this.MarketType = marketType;
            this.TaskStatus = taskStatus;
            this.Notified = notified;
            this.CreatedDate = createdDate;
            this.CreatedBy = createdBy;
            this.BetType = betType;
            this.FinishTime = finishTime;
            this.FinishBy = finishBy;
        }

        public void UpdateTaskMarketTypeAndBetType(MarketType marketType, BetType betType)
        {
            this.MarketType = marketType;
            this.BetType = betType;
        }

        public void UpdateTaskToCancel(OrderTaskStatus customerCanceled, DateTime finishTime, string hitterId)
        {
            this.TaskStatus = customerCanceled;
            this.BetType = BetType.None;//TODO -1
            this.FinishTime = finishTime;
            this.FinishBy = hitterId;
        }
    }
}