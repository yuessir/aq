using System.Collections.Generic;
using Navyblue.BaseLibrary;

namespace AQS.OrderProject.Domain.Reponses
{
    public class CrawlerServiceAccount
    {
        public string CrawlerName { get; set; }

        public List<string> ServiceAccount { get; set; }

        public int ServiceAccountCount { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}