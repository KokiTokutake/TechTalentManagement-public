using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class SelfEvaluationConfiguration : IEntityTypeConfiguration<SelfEvaluation>
{
    public void Configure(EntityTypeBuilder<SelfEvaluation> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => new { s.UserId, s.EvaluationPeriodId }).IsUnique();

        builder.HasOne(s => s.User)
               .WithMany(u => u.SelfEvaluations)
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.EvaluationPeriod)
               .WithMany(p => p.SelfEvaluations)
               .HasForeignKey(s => s.EvaluationPeriodId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Items)
               .WithOne(i => i.SelfEvaluation)
               .HasForeignKey(i => i.SelfEvaluationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.ManagerEvaluation)
               .WithOne(m => m.SelfEvaluation)
               .HasForeignKey<ManagerEvaluation>(m => m.SelfEvaluationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
