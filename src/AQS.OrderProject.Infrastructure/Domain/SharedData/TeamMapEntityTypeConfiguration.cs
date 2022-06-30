using AQS.OrderProject.Domain.SharedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AQS.OrderProject.Infrastructure.Domain.SharedData
{
    public class TeamMapEntityTypeConfiguration : IEntityTypeConfiguration<TeamMap>
    {
        public void Configure(EntityTypeBuilder<TeamMap> builder)
        {
            builder.ToTable("team_map");

            builder.HasKey(x => x.Id);
            builder.Property(p => p.HgName).HasColumnName("hg_name").HasColumnType("VARCHAR(200)");
            builder.Property(p => p.IbcName).HasColumnName("ibc_name").HasColumnType("VARCHAR(200)");
            builder.Property(p => p.SboName).HasColumnName("sbo_name").HasColumnType("VARCHAR(200)");
        }
    }
}