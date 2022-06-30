using System;
using AQS.OrderProject.Domain.SeedWork;

namespace AQS.OrderProject.Domain.Customers.Orders.Events;

public class OrderNotMappingEvent : DomainEventBase
{
    public string UserName { get; set; }

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

    public OrderNotMappingEvent(string userName, OrderId orderId,
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
        this.UserName = userName;
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
}