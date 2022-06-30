using System;
using Navyblue.BaseLibrary;

namespace AQS.OrderProject.Domain.Reponses
{
    public class SearchResult
    {
        public double Rank { get; set; }

        public int MatchId { get; set; }

        public string League { get; set; }

        public string Home { get; set; }

        public string Away { get; set; }

        public int HomeScore { get; set; }

        public int AwayScore { get; set; }

        public string GameTime { get; set; }

        //@JsonFormat(timezone = "GMT+0", pattern = "yyyy-MM-dd'T'HH:mm:ss'Z'")
        public DateTime GameTimeUtc { get; set; }

        public string BetType { get; set; }

        public string RankCalculator { get; set; }

        public long Elapsed { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}