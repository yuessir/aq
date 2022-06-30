using System;

namespace AQS.OrderProject.API;

public class AqsUpdateHitterReq
{
    public string OrderId { get; set; }

    public DateTime CreatedTimeUtc { get; set; }

    public string HitterId { get; set; }
}