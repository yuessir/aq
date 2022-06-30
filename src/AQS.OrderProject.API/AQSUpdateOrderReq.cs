using System;
using System.Collections.Generic;
using AQS.OrderProject.Domain.Customers.Orders;

namespace AQS.OrderProject.API;

public class AqsUpdateOrderReq
{
    public string OrderId { get; set; }

    public DateTime CreatedTime { get; set; }

    public string HitterId { get; set; }

    public long Stake { get; set; }

    public List<AqsOrderLine> OrderLines { get; set; }
}