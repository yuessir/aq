using System;

namespace AQS.OrderProject.Domain.Exceptions
{
    public class DataNotFoundException : Exception
    {
        public string OrderId { get; init; }

        public DateTime CreatedTimeUtc { get; init; }

        public string AgentOrderId { get; init; }

        public int Code { get; init; }

        public DataNotFoundException(string orderId, DateTime createdTimeUtc, string agentOrderId, string message):base(message)
        {
            this.OrderId = orderId;
            this.CreatedTimeUtc = createdTimeUtc;
            this.AgentOrderId = agentOrderId;
        }
    }
}