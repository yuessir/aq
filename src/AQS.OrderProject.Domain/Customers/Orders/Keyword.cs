using Newtonsoft.Json;

namespace AQS.OrderProject.Domain.Customers.Orders;

public class Keyword
{
    [JsonProperty("League")]
    public string[] League { get; set; }

    [JsonProperty("Home")]
    public string[] Home { get; set; }

    [JsonProperty("Away")]
    public string[] Away { get; set; }

    [JsonProperty("GameTime")]
    public string[] GameTime { get; set; }

    [JsonProperty("Choice")]
    public string[] Choice { get; set; }

    public Keyword(OrderMapping[] aqsOrderMappings)
    {
        var length = aqsOrderMappings.Length;
        GameTime = new string[length];
        League = new string[length];
        Home = new string[length];
        Away = new string[length];
        Choice = new string[length];

        for (var i = 0; i < length; i++)
        {
            GameTime[i] = aqsOrderMappings[i].ScheduledKickOffTimeUtc.AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss");
            League[i] = aqsOrderMappings[i].Tournament;
            Home[i] = aqsOrderMappings[i].HomeTeam;
            Away[i] = aqsOrderMappings[i].AwayTeam;
            Choice[i] = aqsOrderMappings[i].Choice;
        }
    }
}