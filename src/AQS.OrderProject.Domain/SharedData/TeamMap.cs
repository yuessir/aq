using AQS.OrderProject.Domain.SeedWork;

namespace AQS.OrderProject.Domain.SharedData
{
    public class TeamMap : Entity, IAggregateRoot
    {
        public string HgName { get; set; }

        public string IbcName { get; set; }

        public string SboName { get; set; }
    }
}