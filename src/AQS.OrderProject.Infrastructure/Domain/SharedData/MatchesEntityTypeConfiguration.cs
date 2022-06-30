using AQS.OrderProject.Domain.SharedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AQS.OrderProject.Infrastructure.Domain.SharedData
{
    public class MatchesEntityTypeConfiguration : IEntityTypeConfiguration<Matches>
    {
        public void Configure(EntityTypeBuilder<Matches> builder)
        {
            builder.ToTable("Matches");

            builder.HasKey(x => x.Id);
            builder.Property(p => p.Website).HasColumnName("web_site").HasColumnType("INT").IsRequired();
            builder.Property(p => p.LeagueId).HasColumnName("leagues_id").HasColumnType("INT").IsRequired();
            builder.Property(p => p.MatchId).HasColumnName("match_id").HasColumnType("VARCHAR(45)");
            builder.Property(p => p.MatchMapId).HasColumnName("match_map_id").HasColumnType("INT");
            builder.Property(p => p.MatchType).HasColumnName("type").HasColumnType("INT").IsRequired();
            builder.Property(p => p.Time).HasColumnName("time").HasColumnType("DATETIME").IsRequired();
            builder.Property(p => p.TimeInfo).HasColumnName("time_info").HasColumnType("VARCHAR(45)");
            builder.Property(p => p.IsLive).HasColumnName("is_live").HasColumnType("BIT(1)").IsRequired();
            builder.Property(p => p.HomeTeamId).HasColumnName("home_team_id").HasColumnType("INT");
            builder.Property(p => p.HomeTeam).HasColumnName("home_team").HasColumnType("VARCHAR(200)").IsRequired();
            builder.Property(p => p.UkHomeTeam).HasColumnName("uk_home_team").HasColumnType("VARCHAR(200)").HasDefaultValue("v2");
            builder.Property(p => p.AwayTeamId).HasColumnName("away_team_id").HasColumnType("INT");
            builder.Property(p => p.AwayTeam).HasColumnName("away_team").HasColumnType("VARCHAR(200)").IsRequired();
            builder.Property(p => p.UkAwayTeam).HasColumnName("uk_away_team").HasColumnType("VARCHAR(200)");
            builder.Property(p => p.Score).HasColumnName("score").HasColumnType("VARCHAR(10)");
            builder.Property(p => p.Reverse).HasColumnName("reverse").HasColumnType("BIT(1)").IsRequired();
            builder.Property(p => p.UnderDog).HasColumnName("under_dog").HasColumnType("INT");
            builder.Property(p => p.ModifyDate).HasColumnName("modify_date").HasColumnType("DATETIME").IsRequired();
        }
    }
}