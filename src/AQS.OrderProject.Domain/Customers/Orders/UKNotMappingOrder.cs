using System;
using AQS.OrderProject.Domain.SeedWork;

namespace AQS.OrderProject.Domain.Customers.Orders
{
    public class UkNotMappingOrder : Entity
    {
        public OrderId OrderId { get; private set; }

        public DateTime CreatedTimeUtc { get; private set; }

        public string RequestJson { get; private set; }

        public DateTime? HgScheduledKickOffTimeUtc { get; private set; }

        public string HgLeagueName { get; private set; }

        public string HgHomeTeam { get; private set; }

        public string HgAwayTeam { get; private set; }

        public DateTime? SboScheduledKickOffTimeUtc { get; private set; }

        public string SboLeagueName { get; private set; }

        public string SboHomeTeam { get; private set; }

        public string SboAwayTeam { get; private set; }

        public DateTime? IbcScheduledKickOffTimeUtc { get; private set; }

        public string IbcLeagueName { get; private set; }

        public string IbcHomeTeam { get; private set; }

        public string IbcAwayTeam { get; private set; }

        private UkNotMappingOrder()
        {

        }

        private UkNotMappingOrder(OrderId orderId,
            DateTime createdTimeUtc,
            string requestJson,
            DateTime? hgScheduledKickOffTimeUtc,
            string hgLeagueName,
            string hgHomeTeam,
            string hgAwayTeam,
            DateTime? sboScheduledKickOffTimeUtc,
            string sboLeagueName,
            string sboHomeTeam,
            string sboAwayTeam,
            DateTime? ibcScheduledKickOffTimeUtc,
            string ibcLeagueName,
            string ibcHomeTeam,
            string ibcAwayTeam)
        {
            this.OrderId = orderId;
            this.CreatedTimeUtc = createdTimeUtc;
            this.RequestJson = requestJson;
            this.HgScheduledKickOffTimeUtc = hgScheduledKickOffTimeUtc;
            this.HgLeagueName = hgLeagueName;
            this.HgHomeTeam = hgHomeTeam;
            this.HgAwayTeam = hgAwayTeam;
            this.SboScheduledKickOffTimeUtc = sboScheduledKickOffTimeUtc;
            this.SboLeagueName = sboLeagueName;
            this.SboHomeTeam = sboHomeTeam;
            this.SboAwayTeam = sboAwayTeam;
            this.IbcScheduledKickOffTimeUtc = ibcScheduledKickOffTimeUtc;
            this.IbcLeagueName = ibcLeagueName;
            this.IbcHomeTeam = ibcHomeTeam;
            this.IbcAwayTeam = ibcAwayTeam;
        }

        public static UkNotMappingOrder CreateNew(OrderId orderId,
            DateTime createdTimeUtc,
            string requestJson,
            DateTime? hgScheduledKickOffTimeUtc,
            string hgLeagueName,
            string hgHomeTeam,
            string hgAwayTeam,
            DateTime? sboScheduledKickOffTimeUtc,
            string sboLeagueName,
            string sboHomeTeam,
            string sboAwayTeam,
            DateTime? ibcScheduledKickOffTimeUtc,
            string ibcLeagueName,
            string ibcHomeTeam,
            string ibcAwayTeam)
        {
            return new UkNotMappingOrder(orderId,
                createdTimeUtc,
                requestJson,
                hgScheduledKickOffTimeUtc,
                hgLeagueName,
                hgHomeTeam,
                hgAwayTeam,
                sboScheduledKickOffTimeUtc,
                sboLeagueName,
                sboHomeTeam,
                sboAwayTeam,
                ibcScheduledKickOffTimeUtc,
                ibcLeagueName,
                ibcHomeTeam,
                ibcAwayTeam);
        }

        public void CreateHgUkNotMappingOrder(Guid orderId,
            DateTime createdTimeUtc,
            string requestJson,
            DateTime hgScheduledKickOffTimeUtc,
            string hgLeagueName,
            string hgHomeTeam,
            string hgAwayTeam)
        {
            this.OrderId = new OrderId(orderId);
            this.CreatedTimeUtc = createdTimeUtc;
            this.RequestJson = requestJson;
            this.HgScheduledKickOffTimeUtc = hgScheduledKickOffTimeUtc;
            this.HgLeagueName = hgLeagueName;
            this.HgHomeTeam = hgHomeTeam;
            this.HgAwayTeam = hgAwayTeam;
        }

        public void CreateSboUkNotMappingOrder(Guid orderId,
            DateTime createdTimeUtc,
            string requestJson,
            DateTime sboScheduledKickOffTimeUtc,
            string sboLeagueName,
            string sboHomeTeam,
            string sboAwayTeam)
        {
            this.OrderId = new OrderId(orderId);
            this.CreatedTimeUtc = createdTimeUtc;
            this.RequestJson = requestJson;
            this.SboScheduledKickOffTimeUtc = sboScheduledKickOffTimeUtc;
            this.SboLeagueName = sboLeagueName;
            this.SboHomeTeam = sboHomeTeam;
            this.SboAwayTeam = sboAwayTeam;
        }

        public void CreateIbcUkNotMappingOrder(Guid orderId,
            DateTime createdTimeUtc,
            string requestJson,
            DateTime ibcScheduledKickOffTimeUtc,
            string ibcLeagueName,
            string ibcHomeTeam,
            string ibcAwayTeam)
        {
            this.OrderId = new OrderId(orderId);
            this.CreatedTimeUtc = createdTimeUtc;
            this.RequestJson = requestJson;
            this.IbcScheduledKickOffTimeUtc = ibcScheduledKickOffTimeUtc;
            this.IbcLeagueName = ibcLeagueName;
            this.IbcHomeTeam = ibcHomeTeam;
            this.IbcAwayTeam = ibcAwayTeam;
        }
    }
}