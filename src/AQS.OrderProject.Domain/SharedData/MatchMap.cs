using System;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.SeedWork;

namespace AQS.OrderProject.Domain.SharedData
{
    public class MatchMap : Entity, IAggregateRoot
    {
        public int HomeTeamId { get; set; }

        public string HomeTeam { get; set; }

        public int AwayTeamId { get; set; }

        public string AwayTeam { get; set; }

        public DateTime Time { get; set; }

        public OrderMappingType Website { get; set; }
    }
}