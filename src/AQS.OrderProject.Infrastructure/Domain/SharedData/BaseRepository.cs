using System;
using System.Linq;
using AQS.OrderProject.Domain.Configs;
using AQS.OrderProject.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AQS.OrderProject.Infrastructure.Domain.SharedData
{
    public class BaseRepository
    {
        private readonly IOptions<WebsiteConfig> _websiteConfig;

        public BaseRepository(IOptions<WebsiteConfig> websiteConfig)
        {
            this._websiteConfig = websiteConfig;
        }

        protected OrdersContext GetContext(string userName)
        {
            var webSite = this._websiteConfig.Value.Websites.FirstOrDefault(p =>
                p.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));

            if (webSite == null)
            {
                throw new Exception($"No dataSource information for this customer {userName}");
            }

            var optionsBuilder = new DbContextOptionsBuilder<OrdersContext>();

            return new OrdersContext(webSite.DataSource.ConnectionString, optionsBuilder.Options);
        }
    }
}