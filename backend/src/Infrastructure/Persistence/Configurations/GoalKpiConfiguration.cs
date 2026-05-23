using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class GoalKpiConfiguration : IEntityTypeConfiguration<GoalKpi>
{
    public void Configure(EntityTypeBuilder<GoalKpi> builder)
    {
        builder.HasKey(k => k.Id);
        builder.Property(k => k.MetricName).IsRequired().HasMaxLength(200);
        builder.Property(k => k.TargetValue).IsRequired().HasPrecision(18, 4);
        builder.Property(k => k.Unit).IsRequired().HasMaxLength(50);
        builder.Property(k => k.MeasurementMethod).HasMaxLength(500);

        builder.HasIndex(k => k.GoalId).IsUnique();

        builder.HasOne(k => k.Goal)
               .WithOne(g => g.GoalKpi)
               .HasForeignKey<GoalKpi>(k => k.GoalId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
