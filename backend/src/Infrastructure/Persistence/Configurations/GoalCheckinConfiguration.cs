using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class GoalCheckinConfiguration : IEntityTypeConfiguration<GoalCheckin>
{
    public void Configure(EntityTypeBuilder<GoalCheckin> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CurrentValue).HasPrecision(18, 4);
        builder.Property(c => c.ProgressPercent).HasPrecision(18, 2);
        builder.Property(c => c.Comment).HasMaxLength(1000);

        builder.HasIndex(c => new { c.GoalId, c.CheckedInAt });

        builder.HasOne(c => c.Goal)
               .WithMany(g => g.GoalCheckins)
               .HasForeignKey(c => c.GoalId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Kpi)
               .WithMany()
               .HasForeignKey(c => c.KpiId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.User)
               .WithMany()
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
