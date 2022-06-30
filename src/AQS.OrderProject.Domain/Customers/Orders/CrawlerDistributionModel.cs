using System;

namespace AQS.OrderProject.Domain.Customers.Orders
{
    public class CrawlerDistributionModel
    {
        public int Id { get; set; }

        public String CrawlerName { get; set; }

        public String Account { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime ModifiedTime { get; set; }
    }
}