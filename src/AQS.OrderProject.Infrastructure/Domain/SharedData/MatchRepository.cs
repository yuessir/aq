using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.SharedData;
using AQS.OrderProject.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AQS.OrderProject.Infrastructure.Domain.SharedData
{
    public class MatchRepository: IMatchRepository
    {
        private readonly SharedContext _sharedContext;
        private readonly ILogger logger;

        public MatchRepository(SharedContext sharedContext,
            ILogger logger)
        {
            this._sharedContext = sharedContext;
            this.logger = logger;
        }

        public async Task<Matches> GetMatch(int matchId)
        {
            return await this._sharedContext.Matches.FirstOrDefaultAsync(p => p.MatchId == matchId);
        }

        public async Task<Matches> FindByWebsiteName(OrderMappingType website, DateTime scheduledKickOffTimeUtc, string homeTeam, string awayTeam, bool isLive)
        {
            return await this._sharedContext.Matches.FirstOrDefaultAsync(p =>
                p.Website == website 
                && p.Time == scheduledKickOffTimeUtc 
                && p.HomeTeam == homeTeam 
                && p.AwayTeam == awayTeam 
                && p.IsLive == isLive);
        }

        public async Task<Matches> FindByUkName(OrderMappingType website, DateTime scheduledKickOffTimeUtc, string homeTeam, string awayTeam, bool isLive)
        {
            return await this._sharedContext.Matches.FirstOrDefaultAsync(p =>
                p.Website == website
                && p.Time == scheduledKickOffTimeUtc
                && p.UkHomeTeam == homeTeam
                && p.UkAwayTeam == awayTeam
                && p.IsLive == isLive);
        }

        public async Task<Matches> FindByUkNameOrWebsiteName(OrderId orderId, OrderMappingType website, string leagueName, DateTime scheduledKickOffTimeUtc, string homeTeam, string awayTeam, bool isLive)
        {
            var league = await this._sharedContext.Leagues.FirstOrDefaultAsync(p => p.Website == website && p.LeagueName == leagueName);
            if (league == null)
            {
                logger.Warning("OrderId: <{}> cannot find mapping league with website: <{}>, leagueName: <{}>", orderId,
                    website, leagueName);
            }
            else
            {
                logger.Information("OrderId: <{}> find mapping league with website: <{}>, leagueId: <{}>, league: <{}>", orderId,
                    website, league.Id, league);
            }

            Matches match = null;
            try
            {
                match = await this.FindByUkName(website, scheduledKickOffTimeUtc, homeTeam, awayTeam, isLive);
                if (match == null)
                {
                    logger.Warning(
                        "OrderId: <{}> cannot find mapping match with website: <{}>, time: <{}>, homeTeam: <{}>, awayTeam: <{}>, isLive: <{}> with UK name",
                        orderId, website, scheduledKickOffTimeUtc, homeTeam, awayTeam, isLive
                    );
                }
            }
            catch (Exception ex)
            {
                logger.Warning(
                    ex is OutOfMemoryException
                        ? "OrderId: <{}> try to find mapping match with website: <{}>, time: <{}>, homeTeam: <{}>, awayTeam: <{}>, isLive: <{}> with UK name but find more than 1 record, so try to find with website name, error-msg: <{}>"
                        : "OrderId: <{}> MyBatisSystemException raised while trying to find mapping match with website: <{}>, time: <{}>, homeTeam: <{}>, awayTeam: <{}>, isLive: <{}> with UK name, error-msg: <{}>",
                    orderId, website, scheduledKickOffTimeUtc, homeTeam, awayTeam, isLive, ex.Message
                );
            }

            if (match == null)
            {
                match = await this.FindByWebsiteName(website, scheduledKickOffTimeUtc, homeTeam, awayTeam, isLive);
                if (match == null)
                {
                    logger.Warning(
                        "OrderId: <{}> cannot find mapping match with website: <{}>, time: <{}>, homeTeam: <{}>, awayTeam: <{}>, isLive: <{}> with website name",
                        orderId, website, scheduledKickOffTimeUtc, homeTeam, awayTeam, isLive
                    );
                }
                else
                {
                    logger.Information("OrderId: <{}> find mapping match with website name by website: <{}>, match: {}", orderId,
                        website, match);
                }
            }
            else
            {
                logger.Information("OrderId: <{}> find mapping match with UK name by website: <{}>, match: {}", orderId, website,
                    match);
            }

            return match;
        }

        public async Task<List<Matches>> FindLastTenMatches()
        {
            return await this._sharedContext.Matches.OrderByDescending(p=>p.Id).Take(10).ToListAsync();
        }

        public async Task UpdateMatchMapId(int matchId, int matchMapId)
        {
            var result = await this._sharedContext.Matches.FirstOrDefaultAsync(p => p.MatchId == matchId);
            if (result != null)
            {
                result.MatchMapId = matchMapId;
                this._sharedContext.Update(result);
                await this._sharedContext.SaveChangesAsync();
            }
        }

        public async Task UpdateMatchMapIdAndTeamId(int matchId, int matchMapId, int homeTeamId, int awayTeamId)
        {
            var result = await this._sharedContext.Matches.FirstOrDefaultAsync(p => p.MatchId == matchId);

            if (result != null)
            {
                result.MatchMapId = matchMapId;
                result.HomeTeamId = homeTeamId;
                result.AwayTeamId = awayTeamId;
                this._sharedContext.Update(result);
                await this._sharedContext.SaveChangesAsync();
            }
        }

        public async Task UpdateTeamMap(int matchId, int matchMapId, int homeTeamId, int awayTeamId)
        {
            var result = await this._sharedContext.Matches.FirstOrDefaultAsync(p => p.MatchId == matchId);

            if (result != null)
            {
                result.MatchMapId = matchMapId;
                result.HomeTeamId = homeTeamId;
                result.AwayTeamId = awayTeamId;
                this._sharedContext.Update(result);
                await this._sharedContext.SaveChangesAsync();
            }
        }

        public async Task UpdateTeamMap(int teamId, OrderMappingType website, string teamName)
        {
            var result = await this._sharedContext.TeamMap.FirstOrDefaultAsync(p => p.Id == teamId);

            if (result != null)
            {
                switch (website)
                {
                    case OrderMappingType.Ibcbet:
                        result.IbcName = website.ToString();
                        break;

                    case OrderMappingType.Sbobet:
                        result.SboName = website.ToString();
                        break;

                    case OrderMappingType.Hg0088:
                    default:
                        break;
                }

                this._sharedContext.Update(result);
                await this._sharedContext.SaveChangesAsync();
            }
        }

        public async Task<int> GetMatchMapIdByTeamName(string homeTeam, string awayTeam, DateTime matchTime)
        {
            var result = await this._sharedContext.MatchMap.FirstOrDefaultAsync(p =>
                p.HomeTeam == homeTeam
                && p.AwayTeam == awayTeam
                && p.Time == matchTime);//TODO the time format

            return result?.Id ?? 0;
        }

        public async Task<int> GetMatchMapIdByTeamId(int homeTeamId, int awayTeamId, DateTime matchTime)
        {
            var result = await this._sharedContext.MatchMap.FirstOrDefaultAsync(p =>
                p.HomeTeamId == homeTeamId
                && p.AwayTeamId == awayTeamId
                && p.Time == matchTime);//TODO the time format

            return (result?.Id).GetValueOrDefault();
        }

        public async Task InsertMatchMap(MatchMap matchMap)
        {
            await this._sharedContext.MatchMap.AddAsync(matchMap);
            await this._sharedContext.SaveChangesAsync();
        }
    }
}