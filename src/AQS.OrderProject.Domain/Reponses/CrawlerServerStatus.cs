using System;
using System.Collections.Generic;
using Navyblue.BaseLibrary;

namespace AQS.OrderProject.Domain.Reponses
{
    public class CrawlerServerStatus
    {
        public String Type { get; set; }

        public List<CrawlerServerDetail> CrawlerServerDetail { get; set; }

        public int ServerCounts { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}