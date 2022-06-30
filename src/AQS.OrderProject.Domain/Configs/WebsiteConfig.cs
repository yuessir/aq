using System;
using System.Collections.Generic;
using System.Linq;

namespace AQS.OrderProject.Domain.Configs
{
    public class WebsiteConfig
    {
        public List<Website> Websites { get; set; }


        public string GetCompanyName(string userName)
        {
            return this.Websites.FirstOrDefault(p => p.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))?.Name;
        }

        public DataSource GetDataSource(string userName)
        {
            return this.Websites.FirstOrDefault(p => p.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))?.DataSource;
        }
    }
}