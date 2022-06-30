using System;
using System.Collections.Generic;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;

namespace AQS.OrderProject.Application.Orders
{
    public class TaskCacheDto
    {
		public int Id{get;set;}

        public int? MatchMapId{get;set;}

        public MarketType MarketType{get;set;}

        public BetType BetType{get;set;}

        public OrderTaskStatus Status{get;set;}

        public DateTime CreatedTime{get;set;}

        public string CreatedBy{get;set;}

        public bool Notified{get;set;}

        public int Seq{get;set;}

        public DateTime GameTime{get;set;}

        public long Stake{get;set;}

        public List<AqsOrderLine> OrderLines{get;set;}
	}
}