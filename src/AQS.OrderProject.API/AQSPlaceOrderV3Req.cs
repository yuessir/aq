using System;
using System.Collections.Generic;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;
using TFA.AQS.Order.Domain.Requests.V3;

namespace AQS.OrderProject.API;

public class AqsPlaceOrderV3Req
{
    public string OrderId { get; set; }

    public AqsMarket Phase { get; set; }

    public string ScoreType { get; set; }

    public AqsStage Stage { get; set; }

    public AqsMarketType MarketType { get; set; }

    public string HitterId { get; set; }

    public long Stake { get; set; }

    public List<OrderMapping> OrderMappings { get; set; }

    public AqsTranslation Translation { get; set; }

    public List<AqsOrderLine> OrderLines { get; set; }
}