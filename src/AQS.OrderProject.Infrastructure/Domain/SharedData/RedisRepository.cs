using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;
using Navyblue.BaseLibrary;
using StackExchange.Redis;
using IDatabase = StackExchange.Redis.IDatabase;

namespace AQS.OrderProject.Infrastructure.Domain.SharedData
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IDatabase _redisDatabase;

        public RedisRepository()
        {
            //this._redisDatabase = Connection.GetDatabase(6);
        }

        public string getRedisKey(RedisCacheCategory category, string companyCode, string key)
        {
            string redisKey
                = new StringBuilder(category.Description()).Append(":")
                            .Append(companyCode).Append(":")
                            .Append(key)
                            .ToString();
            return redisKey;
        }

        public void put<T>(RedisCacheCategory category, string companyCode, string key, T value) where T : class, new()
        {
            string redisKey = getRedisKey(category, companyCode, key);
            var hashEntries = value.ConvertToHashEntryList();

            this._redisDatabase.HashSetAsync(redisKey, hashEntries.ToArray());

            //log.info("[CacheService] Put to redis succeed, key: {}, value: {}", redisKey, redisValue);
        }

        public async Task<T> get<T>(RedisCacheCategory category, string companyCode, string key) where T : class, new()
        {
            string redisKey = getRedisKey(category, companyCode, key);

            var hashEntries = await this._redisDatabase.HashGetAllAsync(redisKey);

            T clazzInRedis = hashEntries.ConvertFromHashEntryList<T>();

            return clazzInRedis;
        }

        public async Task<List<CrawlerDistributionModel>> getAllCrawlerDistribution()
        {
            string redisKey = RedisCacheCategory.CrawlerDistribution.Description() + ":*";

            var keys = await this._redisDatabase.HashKeysAsync(redisKey);

            List<CrawlerDistributionModel> crawlerDists = new List<CrawlerDistributionModel>();

            await keys.ForEach(async key =>
            {
                var values = await this._redisDatabase.HashGetAllAsync(new RedisKey(key));
                CrawlerDistributionModel crawlerDist = values.ConvertFromHashEntryList<CrawlerDistributionModel>();
                crawlerDists.Add(crawlerDist);
            });

            return crawlerDists;
        }

        public async Task<List<CrawlerServer>> getAllCrawlerServers()
        {
            string redisKey = RedisCacheCategory.CrawlerServer.Description() + ":*";

            var keys = await this._redisDatabase.HashKeysAsync(redisKey);

            List<CrawlerServer> crawlerServers = new List<CrawlerServer>();

            await keys.ForEach(async key =>
            {
                var values = await this._redisDatabase.HashGetAllAsync(new RedisKey(key));
                CrawlerServer crawlerServer = values.ConvertFromHashEntryList<CrawlerServer>();
                crawlerServers.Add(crawlerServer);
            });

            return crawlerServers;
        }

        public async Task<bool> exists(RedisCacheCategory category, string companyCode, string key)
        {
            string redisKey = getRedisKey(category, companyCode, key);

            return await this._redisDatabase.KeyExistsAsync(redisKey);
        }

        public async Task<string> remove(RedisCacheCategory category, string companyCode, string key)
        {
            if (await exists(category, companyCode, key))
            {
                string redisKey = getRedisKey(category, companyCode, key);

                await this._redisDatabase.KeyDeleteAsync(redisKey);

                return redisKey;
            }
            return null;
        }

        //private static readonly Lazy<ConnectionMultiplexer> lazyConnection = CreateConnection();

        //public static ConnectionMultiplexer Connection => lazyConnection.Value;

        //private static Lazy<ConnectionMultiplexer> CreateConnection()
        //{
        //    return new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect("cache.dev.uw2.gdotawsnp.com:6379"));
        //}
    }

}