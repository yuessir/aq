using System;
using System.Collections.Generic;
using AQS.OrderProject.Domain.Customers.Orders;
using TFA.AQS.Order.Domain.Requests.V3;

namespace AQS.OrderProject.API;

public class AqsPrepareOrderReq
{
    public string OrderId { get; set; }

    public string HitterId { get; set; }

    public List<OrderMapping> OrderMappings { get; set; }

    public AqsTranslation Translation { get; set; }

    /// <summary>
    /// 收到 request 時塞入
    /// </summary>
    public DateTime ReceivedReqTime { get; set; }
}