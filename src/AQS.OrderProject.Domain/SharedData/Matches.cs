using System;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.SeedWork;

namespace AQS.OrderProject.Domain.SharedData
{
    public class Matches : Entity, IAggregateRoot
    {
        public OrderMappingType Website { get; set; }

        public int LeagueId { get; set; }

        public int MatchId { get; set; }

        public int MatchMapId { get; set; }

        public MatchType MatchType { get; set; }

        public DateTime Time { get; set; }

        public string TimeInfo { get; set; }

        public bool IsLive { get; set; }

        public int HomeTeamId { get; set; }

        public string HomeTeam { get; set; }

        public string UkHomeTeam { get; set; }

        public int AwayTeamId { get; set; }

        public string AwayTeam { get; set; }

        public string UkAwayTeam { get; set; }

        public string Score { get; set; }

        public int Reverse { get; set; }

        public UnderDog UnderDog { get; set; }

        public DateTime ModifyDate { get; set; }
	}
}