using Navyblue.BaseLibrary;

namespace AQS.OrderProject.Domain.Reponses
{
    public class AqsResponse
    {
		public string OrderId { get; set; }

        public string CreatedTimeUtc { get; set; }

        public string AgentOrderId { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}