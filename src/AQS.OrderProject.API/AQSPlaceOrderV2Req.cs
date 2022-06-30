using System;
using System.Collections.Generic;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;

namespace AQS.OrderProject.API;

public class AqsPlaceOrderV2Req
{
    public string OrderId { get; set; }

    public DateTime CreatedTimeUtc { get; set; }

    public AqsMarket Phase { get; set; }

    public string ScoreType { get; set; }

    public AqsStage Stage { get; set; }

    public AqsMarketType MarketType { get; set; }

    public string HitterId { get; set; }

    public List<OrderMapping> OrderMappings { get; set; }
}