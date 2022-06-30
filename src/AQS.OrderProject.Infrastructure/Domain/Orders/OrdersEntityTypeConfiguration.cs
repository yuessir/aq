using System;
using AQS.OrderProject.Domain.Customers.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AQS.OrderProject.Infrastructure.Domain.Orders
{
    public class OrdersEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(x => x.Id);
            builder.Property(p => p.OrderId).HasColumnName("order_id").HasColumnType("CHAR(36)").IsRequired().HasConversion(new OrderIdValueConverter());
            builder.Property(p => p.CreatedTime).HasColumnName("created_time").HasColumnType("DATETIME").IsRequired();
            builder.Property(p => p.Market).HasColumnName("market").HasColumnType("INT").IsRequired();
            builder.Property(p => p.MatchType).HasColumnName("match_type").HasColumnType("INT").IsRequired();
            builder.Property(p => p.MarketType).HasColumnName("market_type").HasColumnType("INT").IsRequired();
            builder.Property(p => p.CreatedBy).HasColumnName("created_by").HasColumnType("VARCHAR(45)").IsRequired();
            builder.Property(p => p.OrderMappings).HasColumnName("order_mappings").HasColumnType("TEXT").IsRequired();
            builder.Property(p => p.MatchMapId).HasColumnName("match_map_id").HasColumnType("INT");
            builder.Property(p => p.OrderLines).HasColumnName("order_lines").HasColumnType("TEXT");
            builder.Property(p => p.OrderVersion).HasColumnName("order_version").HasColumnType("CHAR(3)").HasDefaultValue("v2").IsRequired();
            builder.Property(p => p.OrderStatus).HasColumnName("status").HasColumnType("INT").IsRequired();
            builder.Property(p => p.TaskId).HasColumnName("task_id").HasColumnType("INT").IsRequired();
            builder.Property(p => p.AgentId).HasColumnName("agent_id").HasColumnType("CHAR(36)").IsRequired();
            builder.Property(p => p.Score).HasColumnName("Score").HasColumnType("VARCHAR(10)");
            builder.Property(p => p.UpdateTime).HasColumnName("update_time").HasColumnType("DATETIME");
        }
    }

    public class OrderIdValueConverter : ValueConverter<OrderId, Guid>
    {
        public OrderIdValueConverter(ConverterMappingHints mappingHints = null)
            : base(
                id => id.Value,
                value => new OrderId(value),
                mappingHints
            )
        { }
    }
}