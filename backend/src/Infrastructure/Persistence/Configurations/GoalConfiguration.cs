using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Title).HasMaxLength(200).IsRequired();
        builder.Property(g => g.Description).HasMaxLength(2000);
        builder.Property(g => g.Category).IsRequired();

        builder.HasOne(g => g.User)
               .WithMany(u => u.Goals)
               .HasForeignKey(g => g.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.EvaluationPeriod)
               .WithMany(p => p.Goals)
               .HasForeignKey(g => g.EvaluationPeriodId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => new { g.UserId, g.EvaluationPeriodId, g.Category });
    }
}
