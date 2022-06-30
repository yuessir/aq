using System;

namespace AQS.OrderProject.API;

public class AqsCancelOrderReq
{
    public string OrderId { get; set; }

    public string CreatedTimeUtc { get; set; }

    public string HitterId { get; set; }
}