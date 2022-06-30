using System;

namespace AQS.OrderProject.Domain.Customers.Orders
{
    public class OrderMapping
    {
		public AqsBook Book { get; set; }

        public string Tournament { get; set; }

        public string HomeTeam { get; set; }

        public string AwayTeam { get; set; }

        //@JsonFormat(timezone = "GMT+0", pattern = "yyyy-MM-dd'T'HH:mm:ss'Z'")
        public DateTime ScheduledKickOffTimeUtc { get; set; }

        public string Choice { get; set; }

        public int LiveHomeScore { get; set; }

        public int LiveAwayScore { get; set; }
    }
}