using System;
using AQS.OrderProject.Domain.Customers.Orders.Events;
using AQS.OrderProject.Domain.Customers.Rules;
using AQS.OrderProject.Domain.SeedWork;
using Navyblue.BaseLibrary;

namespace AQS.OrderProject.Domain.Customers.Orders
{
    public class Order : Entity, IAggregateRoot
    {
        public OrderId OrderId { get; private set; }

        public DateTime CreatedTime { get; private set; }

        public Market Market { get; private set; }

        public MatchType MatchType { get; private set; }

        public MarketType MarketType { get; private set; }

        public BetType BetType { get; private set; }

        public string CreatedBy { get; private set; }

        public string OrderMappings { get; private set; }

        public string OrderLines { get; private set; }

        public string OrderVersion { get; private set; }

        public int? MatchMapId { get; private set; }

        public OrderStatus OrderStatus { get; private set; }

        /// <summary>
        /// set after insert task
        /// </summary>
        public int TaskId { get; private set; }

        /// <summary>
        /// our system processing id
        /// </summary>
        public string AgentId { get; private set; }

        public string Score { get; private set; }

        public DateTime UpdateTime { get; private set; }

        private Order()
        {

        }

        private Order(OrderId orderId,
            MatchType matchType,
            string orderMappings,
            string orderLines,
            string orderVersion,
            int matchMapId,
            OrderStatus orderStatus,
            int taskId,
            string agentId,
            string score,
            string createdBy,
            DateTime createdTime,
            DateTime updateTime,
            AqsMarket aqsMarket,
            AqsStage stage,
            AqsMarketType aqsMarketType,
            string choice,
            string homeTeam,
            string awayTeam,
            Market? market = null,
            MarketType? marketType = null,
            BetType? betType = null
            )
        {
            this.OrderId = orderId;
            this.MatchType = matchType;
            this.OrderMappings = orderMappings;
            this.OrderLines = orderLines;
            this.OrderVersion = orderVersion;
            this.MatchMapId = matchMapId;
            this.OrderStatus = orderStatus;
            this.TaskId = taskId;
            this.AgentId = agentId;
            this.Score = score;
            this.CreatedBy = createdBy;
            this.CreatedTime = createdTime;
            this.UpdateTime = updateTime;

            if (market == null)
            {
                this.SetMarket(aqsMarket);
            }
            else
            {
                this.Market = market.Value;
            }

            if (marketType == null)
            {
                this.SetMarketType(stage, aqsMarketType);
            }
            else
            {
                this.MarketType = marketType.Value;
            }

            if (betType == null)
            {
                this.SetBetType(this.MarketType, choice, homeTeam, awayTeam);
            }
            else
            {
                this.BetType = betType.Value;
            }

            this.AddDomainEvent(new OrderPlacedEvent(orderId,
                this.Market,
                this.MatchType,
                this.MarketType,
                this.BetType,
                this.CreatedBy,
                this.OrderLines,
                this.OrderVersion,
                this.MatchMapId,
                this.OrderStatus,
                this.TaskId,
                this.AgentId,
                this.Score,
                this.OrderMappings,
                this.CreatedTime,
                this.UpdateTime));
        }

        /// <summary>
        /// 由于TaskId强关联Task，此处不能直接返回OrderId，后期需要优化
        /// </summary>
        public static Order PlaceOrder(string userName, OrderId orderId,
            MatchType matchType,
            string orderMappings,
            string orderLines,
            string orderVersion,
            int matchMapId,
            OrderStatus orderStatus,
            int taskId,
            string agentId,
            string score,
            string createdBy,
            DateTime createdTime,
            DateTime updateTime,
            AqsMarket aqsMarket,
            AqsStage stage,
            AqsMarketType aqsMarketType,
            string choice,
            string homeTeam,
            string awayTeam,
            IOrderDuplicateChecker orderDuplicateChecker)
        {
            CheckRule(new OrderMustBeUnduplicatedRule(orderId.Value.ToGuidString(), orderDuplicateChecker, userName));

            var order = Order.CreateNew(userName, orderId,
                matchType,
                orderMappings,
                orderLines,
                orderVersion,
                matchMapId,
                orderStatus,
                taskId,
                agentId,
                score,
                createdBy,
                createdTime,
                updateTime,
                aqsMarket,
                stage,
                aqsMarketType,
                choice,
                homeTeam,
                awayTeam);

            order.AddDomainEvent(new OrderPlacedEvent(orderId,
                order.Market,
                order.MatchType,
                order.MarketType,
                order.BetType,
                order.CreatedBy,
                order.OrderLines,
                order.OrderVersion,
                order.MatchMapId,
                order.OrderStatus,
                order.TaskId,
                order.AgentId,
                order.Score,
                order.OrderMappings,
                order.CreatedTime,
                order.UpdateTime));

            return order;
        }

        /// <summary>
        /// 由于TaskId强关联Task，此处不能直接返回OrderId，后期需要优化
        /// </summary>
        public static Order PlacePreparedOrder(string userName, OrderId orderId,
            string orderMappings,
            string orderLines,
            string orderVersion,
            int matchMapId,
            OrderStatus orderStatus,
            int taskId,
            string agentId,
            string score,
            string createdBy,
            IOrderDuplicateChecker orderDuplicateChecker)
        {
            CheckRule(new OrderMustBeUnduplicatedRule(orderId.Value.ToGuidString(), orderDuplicateChecker, userName));

            var order = CreateNew(userName, orderId,
                MatchType.Normal,// FIXME AQS V3 - EDS Order 的 MatchType 可能要根據 UK 來的 Score Type 來決定, 但不是很重要
                orderMappings,
                orderLines,
                orderVersion,
                matchMapId,
                orderStatus,
                taskId,
                agentId,
                score,
                createdBy,
                DateTime.UtcNow,
                DateTime.UtcNow,
                AqsMarket.PreMatch,//任意值，后续需要优化
                AqsStage.FirstHalf,//任意值，后续需要优化
                AqsMarketType.AsianHandicap,//任意值，后续需要优化
                null,
                null,
                null,
                Market.None,
                MarketType.None,
                BetType.None);

            order.AddDomainEvent(new OrderPlacedEvent(orderId,
                order.Market,
                order.MatchType,
                order.MarketType,
                order.BetType,
                order.CreatedBy,
                order.OrderLines,
                order.OrderVersion,
                order.MatchMapId,
                order.OrderStatus,
                order.TaskId,
                order.AgentId,
                order.Score,
                order.OrderMappings,
                order.CreatedTime,
                order.UpdateTime));

            return order;
        }

        /// <summary>
        /// 由于TaskId强关联Task，此处不能直接返回OrderId，后期需要优化
        /// </summary>
        public static void PlaceUkNotMappingOrder(string userName, OrderId orderId,
            DateTime createdTimeUtc,
            string requestJson,
            DateTime? hgScheduledKickOffTimeUtc,
            string hgLeagueName,
            string hgHomeTeam,
            string hgAwayTeam,
            DateTime? sboScheduledKickOffTimeUtc,
            string sboLeagueName,
            string sboHomeTeam,
            string sboAwayTeam,
            DateTime? ibcScheduledKickOffTimeUtc,
            string ibcLeagueName,
            string ibcHomeTeam,
            string ibcAwayTeam)
        {
            new Order().AddDomainEvent(new OrderNotMappingEvent(userName, orderId,
                createdTimeUtc,
                requestJson,
                hgScheduledKickOffTimeUtc,
                hgLeagueName,
                hgHomeTeam,
                hgAwayTeam,
                sboScheduledKickOffTimeUtc,
                sboLeagueName,
                sboHomeTeam,
                sboAwayTeam,
                ibcScheduledKickOffTimeUtc,
                ibcLeagueName,
                ibcHomeTeam,
                ibcAwayTeam));
        }

        internal static Order CreateNew(string userName, OrderId orderId,
            MatchType matchType,
            string orderMappings,
            string orderLines,
            string orderVersion,
            int matchMapId,
            OrderStatus orderStatus,
            int taskId,
            string agentId,
            string score,
            string createdBy,
            DateTime createdTime,
            DateTime updateTime,
            AqsMarket aqsMarket,
            AqsStage stage,
            AqsMarketType aqsMarketType,
            string choice,
            string homeTeam,
            string awayTeam,
            Market? market = null,
            MarketType? marketType = null,
            BetType? betType = null)
        {
            return new Order(orderId,
                matchType,
                orderMappings,
                orderLines,
                orderVersion,
                matchMapId,
                orderStatus,
                taskId,
                agentId,
                score,
                createdBy,
                createdTime,
                updateTime,
                aqsMarket,
                stage,
                aqsMarketType,
                choice,
                homeTeam,
                awayTeam,
                market,
                marketType,
                betType);
        }

        private void SetMarket(AqsMarket aqsMarket)
        {
            switch (aqsMarket)
            {
                case AqsMarket.PreMatch:
                    this.Market = Market.Today;
                    break;

                case AqsMarket.InRunning:
                    this.Market = Market.Live;
                    break;

                default:
                    this.Market = Market.None;
                    break;
            }
        }

        private void SetMarketType(AqsStage aqsStage, AqsMarketType aqsMarketType)
        {
            switch (aqsStage)
            {
                case AqsStage.FirstHalf:
                    this.MarketType = aqsMarketType switch
                    {
                        AqsMarketType.AsianHandicap => MarketType.HtHdp,
                        AqsMarketType.OverUnder => MarketType.HtOu,
                        _ => this.MarketType
                    };
                    break;

                case AqsStage.FullTime:
                    this.MarketType = aqsMarketType switch
                    {
                        AqsMarketType.AsianHandicap => MarketType.FtHdp,
                        AqsMarketType.OverUnder => MarketType.FtOu,
                        _ => this.MarketType
                    };
                    break;

                case AqsStage.FullTimeExtraTime:
                    // FIXME EDS Order 的 MatchType 應該要根據 UK stage 塞入, 目前系統沒 FullTimeExtraTime
                    break;

                case AqsStage.Penalties:
                    // FIXME EDS Order 的 MatchType 應該要根據 UK stage 塞入, 目前系統沒 Penalties
                    break;

                case AqsStage.FirstHalfExtraTime:
                    // FIXME EDS Order 的 MatchType 應該要根據 UK stage 塞入, 目前系統沒 FirstHalfExtraTime
                    break;

                default:
                    this.MarketType = MarketType.None;
                    break;
            }
        }

        private void SetBetType(MarketType marketType, string choice, string homeTeam, string awayTeam)
        {
            switch (marketType)
            {
                case MarketType.HtHdp:
                case MarketType.FtHdp:
                    if (choice.Equals(homeTeam))
                    {
                        this.BetType = BetType.Home;
                    }
                    else if (choice.Equals(awayTeam))
                    {
                        this.BetType = BetType.Away;
                    }
                    break;

                case MarketType.HtOu:
                case MarketType.FtOu:
                    if (choice.Equals("over", StringComparison.OrdinalIgnoreCase))
                    {
                        this.BetType = BetType.Over;
                    }
                    else if (choice.Equals("under", StringComparison.OrdinalIgnoreCase))
                    {
                        this.BetType = BetType.Under;
                    }
                    break;

                default:
                    this.BetType = BetType.None;
                    break;
            }
        }

        public void SetTaskId(int taskId)
        {
            this.TaskId = taskId;
        }

        public void Change(Market market, MarketType marketType, string orderMappings, string orderLines, OrderStatus orderStatus, string createBy, DateTime? dateTime = null)
        {
            this.Market = market;
            this.MarketType = marketType;
            this.OrderMappings = orderMappings;
            this.OrderLines = orderLines;
            this.OrderStatus = orderStatus;
            this.CreatedBy = createBy;
            this.UpdateTime = dateTime ?? DateTime.UtcNow;

            AddDomainEvent(new OrderChangedEvent(this.OrderId));
        }

        public void Cancel(string currentUser, OrderId orderId, string hitterId, string companyName)
        {
            this.OrderStatus = OrderStatus.Deleted;
            this.UpdateTime = DateTime.UtcNow;

            AddDomainEvent(new OrderCancelledEvent(orderId, currentUser, hitterId, companyName));
        }
    }
}