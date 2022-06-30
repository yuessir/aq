using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.SeedWork;

namespace AQS.OrderProject.Domain.SharedData
{
    public class Leagues : Entity, IAggregateRoot
    {
        public string LeagueId { get; set; }

        public int LeagueMapId { get; set; }

        public string LeagueName { get; set; }

        public OrderMappingType Website { get; set; }

        public int Sport { get; set; }

    }
}