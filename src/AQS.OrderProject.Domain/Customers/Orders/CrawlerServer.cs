using System;

namespace AQS.OrderProject.Domain.Customers.Orders
{
    public class CrawlerServer
    {
        public String Name { get; set; }

        public String Server { get; set; }

        public String Type { get; set; }

        public String Status { get; set; }

        public int Id { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime ModifiedTime { get; set; }

    }
}