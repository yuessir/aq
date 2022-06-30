using System;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.Reponses;
using AQS.OrderProject.Domain.SharedData;
using AQS.OrderProject.Infrastructure;
using Autofac;
using Navyblue.BaseLibrary;
using Serilog;

namespace AQS.OrderProject.Application.Orders
{
    public class MatchProvider
    {
        public static async Task<Tuple<OrderMapping, Matches>> ProcessMatch(OrderId orderId, string host, bool isLive,
            OrderMapping hgOrderMapping,
            OrderMapping sboOrderMapping, OrderMapping ibcOrderMapping,
            ILogger logger)
        {
            using var lifetimeScope = CompositionRoot.BeginLifetimeScope();

            var matchRepository = lifetimeScope.Resolve<IMatchRepository>();

            Matches hgMatch = null;
            if (hgOrderMapping != null)
            {
                hgMatch = await matchRepository.FindByUkNameOrWebsiteName(orderId,
                    OrderMappingType.Hg0088,
                    hgOrderMapping.Tournament,
                    hgOrderMapping.ScheduledKickOffTimeUtc,
                    hgOrderMapping.HomeTeam,
                    hgOrderMapping.AwayTeam,
                    isLive);

                if (hgMatch == null)
                {
                    var keyword = GetKeyword(hgOrderMapping, sboOrderMapping, ibcOrderMapping, logger);
                    var searchResult = await GetSearchResult(keyword, logger, host);

                    //when match got -1, mean match can't be found, then throw 404 exception
                    if (searchResult.MatchId != -1)
                    {
                        hgOrderMapping = GetNewHgAqsOrderMapping(hgOrderMapping.Book, searchResult, logger);

                        hgMatch = await matchRepository.GetMatch(searchResult.MatchId);
                        logger.Information("[ProcessMatch] Got match: <{}>", hgMatch.ToJson());
                    }
                    else
                    {
                        //when matchId got -1, raised exception is 500 (before fix)
                        logger.Information("[ProcessMatch] OrderId: <{}> match cannot be searched", orderId);

                        return new Tuple<OrderMapping, Matches>(null, null);
                    }
                }
            }

            Matches sboMatch = null;
            if (sboOrderMapping != null)
            {
                sboMatch = await matchRepository.FindByUkNameOrWebsiteName(orderId,
                    OrderMappingType.Sbobet,
                    sboOrderMapping.Tournament,
                    sboOrderMapping.ScheduledKickOffTimeUtc,
                    sboOrderMapping.HomeTeam,
                    sboOrderMapping.AwayTeam,
                    isLive);
            }

            Matches ibcMatch = null;
            if (ibcOrderMapping != null)
            {
                ibcMatch = await matchRepository.FindByUkNameOrWebsiteName(orderId,
                    OrderMappingType.Ibcbet,
                    ibcOrderMapping.Tournament,
                    ibcOrderMapping.ScheduledKickOffTimeUtc,
                    ibcOrderMapping.HomeTeam,
                    ibcOrderMapping.AwayTeam,
                    isLive);
            }

            // 若 HG match 不為 null 拿其去更新資料庫資訊
            if (hgMatch != null)
            {
                // 更新 SBO 相關賽事資訊
                if (sboMatch is { MatchMapId: 0 })
                {
                    // Note:因為有可能 HomeTeam 跟 AwayTeam 是相反的, 所以不做 team id 的更新
                    //      sboMatch.setHomeTeamId(hgMatch.HomeTeamId());
                    //      sboMatch.setAwayTeamId(hgMatch.AwayTeamId());
                    //      updateMatchMapIdAndTeamInfo(WebSite.SBOBET, sboMatch.Id, sboMatch.MatchMapIdByTeamName, sboMatch.HomeTeamId, sboMatch.HomeTeam, sboMatch.AwayTeamId, sboMatch.AwayTeam());

                    UpdateMatchMapId(orderId, sboMatch, hgMatch.MatchMapId, matchRepository, logger);
                }

                // 更新 IBC 相關賽事資訊
                if (ibcMatch is { MatchMapId: 0 })
                {
                    // Note:因為有可能 HomeTeam 跟 AwayTeam 是相反的, 所以不做 team id 的更新
                    //      ibcMatch.setHomeTeamId(hgMatch.HomeTeamId());
                    //      ibcMatch.setAwayTeamId(hgMatch.AwayTeamId());
                    //      updateMatchMapIdAndTeamInfo(WebSite.IBCBET, ibcMatch.Id, ibcMatch.MatchMapIdByTeamName, ibcMatch.HomeTeamId, ibcMatch.HomeTeam, ibcMatch.AwayTeamId, ibcMatch.AwayTeam());

                    UpdateMatchMapId(orderId, ibcMatch, hgMatch.MatchMapId, matchRepository, logger);
                }
            }
            // 若 HG match 為 null, 只發了 SBO & IBC, 甚至只發了單獨一場, 要幫忙建立 match_map
            else
            {
                // 根據 SBO 資訊建立 match map
                if (sboMatch is { MatchMapId: 0 })
                {
                    var matchMapId = 0;

                    // Find match map id by SBO home team id + away team id + match time
                    if (sboMatch.HomeTeamId > 0 && sboMatch.AwayTeamId > 0)
                    {
                        matchMapId = await matchRepository.GetMatchMapIdByTeamId(sboMatch.HomeTeamId, sboMatch.AwayTeamId, sboMatch.Time);
                    }

                    // Find match map id by SBO home team name + away team name + match time
                    if (matchMapId == 0)
                    {
                        matchMapId = await matchRepository.GetMatchMapIdByTeamName(sboMatch.HomeTeam, sboMatch.AwayTeam, sboMatch.Time);
                    }

                    if (matchMapId == 0)
                    {
                        MatchMap matchMap = new()
                        {
                            HomeTeamId = sboMatch.HomeTeamId,
                            HomeTeam = sboMatch.HomeTeam,
                            AwayTeamId = sboMatch.AwayTeamId,
                            AwayTeam = sboMatch.AwayTeam,
                            Time = sboMatch.Time,
                            Website = OrderMappingType.Sbobet
                        };

                        await matchRepository.InsertMatchMap(matchMap);

                        logger.Information("OrderId: <{}> insert match map succeed, content: {}", orderId, matchMap);

                        UpdateMatchMapId(orderId, sboMatch, matchMap.Id, matchRepository, logger);
                        if (ibcMatch != null)
                        {
                            UpdateMatchMapId(orderId, ibcMatch, matchMap.Id, matchRepository, logger);
                        }
                    }
                    else
                    {
                        UpdateMatchMapId(orderId, sboMatch, matchMapId, matchRepository, logger);
                        if (ibcMatch != null)
                        {
                            UpdateMatchMapId(orderId, ibcMatch, matchMapId, matchRepository, logger);
                        }
                    }
                }
                // 找不到 SBO Match
                else
                {
                    // 根據 IBC 資訊建立 match map
                    if (ibcMatch is { MatchMapId: 0 })
                    {
                        var matchMapId = 0;

                        // Find match map id by IBC home team id + away team id + match time
                        if (ibcMatch.HomeTeamId > 0 && ibcMatch.AwayTeamId > 0)
                        {
                            matchMapId = await matchRepository.GetMatchMapIdByTeamId(ibcMatch.HomeTeamId, ibcMatch.AwayTeamId, ibcMatch.Time);
                        }

                        // Find match map id by IBC home team name + away team name + match time
                        if (matchMapId == 0)
                        {
                            matchMapId = await matchRepository.GetMatchMapIdByTeamName(ibcMatch.HomeTeam, ibcMatch.AwayTeam, ibcMatch.Time);
                        }

                        if (matchMapId == 0)
                        {
                            MatchMap matchMap = new()
                            {
                                HomeTeamId = ibcMatch.HomeTeamId,
                                HomeTeam = ibcMatch.HomeTeam,
                                AwayTeamId = ibcMatch.AwayTeamId,
                                AwayTeam = ibcMatch.AwayTeam,
                                Time = ibcMatch.Time,
                                Website = OrderMappingType.Ibcbet
                            };

                            await matchRepository.InsertMatchMap(matchMap);

                            logger.Information("OrderId: <{}> insert match map succeed, content: {}", orderId, matchMap);

                            UpdateMatchMapId(orderId, ibcMatch, matchMap.Id, matchRepository, logger);
                        }
                        else
                        {
                            UpdateMatchMapId(orderId, ibcMatch, matchMapId, matchRepository, logger);
                        }
                    }
                    else
                    {
                        logger.Warning(
                            "[ProcessMatch] OrderId: <{}> cannot find any match from system database with HG, SBO, IBC",
                            orderId);
                    }
                }
            }

            // 依據 hg -> sbo -> ibc 順序, 不為 null 就拿他當參考物件
            OrderMapping refOrderMapping = null;

            if (hgOrderMapping != null)
            {
                refOrderMapping = hgOrderMapping;
            }
            else if (sboOrderMapping != null)
            {
                refOrderMapping = sboOrderMapping;
            }
            else if (ibcOrderMapping != null)
            {
                refOrderMapping = ibcOrderMapping;
            }

            logger.Information("[ProcessMatch] OrderId: <{}> reference order mapping: {}", orderId, refOrderMapping);

            Matches refMatch = null;
            if (hgMatch != null)
            {
                refMatch = hgMatch;
            }
            else if (sboMatch != null)
            {
                refMatch = sboMatch;
            }
            else if (ibcMatch != null)
            {
                refMatch = ibcMatch;
            }

            logger.Information("[ProcessMatch] OrderId: <{}> reference match: {}", orderId, refMatch);

            return new Tuple<OrderMapping, Matches>(refOrderMapping, refMatch);
        }

        private static Keyword GetKeyword(OrderMapping hgOrderMapping, OrderMapping sboOrderMapping,
            OrderMapping ibcOrderMapping, ILogger logger)
        {
            ArrayList aqsOrderMappings = new();

            if (hgOrderMapping != null)
            {
                aqsOrderMappings.Add(hgOrderMapping);
            }

            if (sboOrderMapping != null)
            {
                aqsOrderMappings.Add(sboOrderMapping);
            }

            if (ibcOrderMapping != null)
            {
                aqsOrderMappings.Add(ibcOrderMapping);
            }

            var aqsOrderMappingArr = new OrderMapping[aqsOrderMappings.Count];
            aqsOrderMappings.CopyTo(aqsOrderMappingArr);
            Keyword keyword = new(aqsOrderMappingArr);

            logger.Information("[GameSearching] Got Keyword: {}", keyword.ToJson());
            return keyword;
        }

        private static OrderMapping GetNewHgAqsOrderMapping(AqsBook aqsBook, SearchResult searchResult, ILogger logger)
        {
            OrderMapping newOrderMapping = new()
            {
                Tournament = searchResult.League,
                HomeTeam = searchResult.Home,
                AwayTeam = searchResult.Away,
                Book = aqsBook,
                Choice = searchResult.BetType,
                LiveHomeScore = searchResult.HomeScore,
                LiveAwayScore = searchResult.AwayScore,
                ScheduledKickOffTimeUtc = searchResult.GameTimeUtc
            };

            logger.Information("[GameSearching] Got new AQSOrderMapping: {}", newOrderMapping.ToString());
            return newOrderMapping;
        }

        private static void UpdateMatchMapId(OrderId orderId, Matches match, int matchMapId, IMatchRepository matchRepository, ILogger logger)
        {
            match.MatchMapId = matchMapId;

            matchRepository.UpdateMatchMapId(match.Id, matchMapId);

            logger.Information("OrderId: <{}> update matches website: <{}>, matchId: <{}> with matchMapId: <{}> succeed", orderId,
                match.Website, match.Id, matchMapId);
        }

        private static async Task<SearchResult> GetSearchResult(Keyword keyword, ILogger logger, string host)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(host);
            var response = await httpClient.PostAsJsonAsync("/api/match/search/1", keyword);
            var result = await response.Content.ReadAsStringAsync();
            var searchResult = result.FromJson<SearchResult>();

            logger.Information($"[GameSearching] Searching url=<{httpClient.BaseAddress + "/api/match/search/1"}> got result: {searchResult.ToJson()}");
            return searchResult;
        }
    }
}