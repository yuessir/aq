using System;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;

namespace AQS.OrderProject.Application.Orders
{
    public class TasksModel
    {
        public int Id { get; set; }

        public int Seq { get; set; }

        public int MatchId { get; set; }

        public int? MatchMapId { get; set; }

        public MarketType MarketType { get; set; }

        public OrderTaskStatus TaskStatus { get; set; }

        public bool Notified { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public BetType BetType { get; set; }

        public DateTime FinishTime { get; set; }

        public string FinishBy { get; set; }
    }
}