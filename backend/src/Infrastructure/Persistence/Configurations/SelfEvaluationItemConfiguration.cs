using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class SelfEvaluationItemConfiguration : IEntityTypeConfiguration<SelfEvaluationItem>
{
    public void Configure(EntityTypeBuilder<SelfEvaluationItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.AchievementScore).IsRequired();
        builder.Property(i => i.Comment).HasMaxLength(2000);

        builder.HasIndex(i => new { i.SelfEvaluationId, i.GoalId }).IsUnique();

        builder.HasOne(i => i.Goal)
               .WithOne(g => g.SelfEvaluationItem)
               .HasForeignKey<SelfEvaluationItem>(i => i.GoalId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
