using System.ComponentModel;

namespace AQS.OrderProject.Domain.Customers.Orders
{
    public enum RedisCacheCategory
    {
        [Description("Data:Task")]
        Task,

        [Description("Crawler:Distribution")]
        CrawlerDistribution,

        [Description("Crawler:Server")]
        CrawlerServer
    }
}