using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class CompetencyEvaluationConfiguration : IEntityTypeConfiguration<CompetencyEvaluation>
{
    public void Configure(EntityTypeBuilder<CompetencyEvaluation> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Comment).HasMaxLength(500);

        builder.HasIndex(e => new { e.TalentId, e.EvaluatorId, e.CompetencyId, e.PeriodId, e.EvaluatorType }).IsUnique();

        builder.HasOne(e => e.Talent)
            .WithMany()
            .HasForeignKey(e => e.TalentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Evaluator)
            .WithMany()
            .HasForeignKey(e => e.EvaluatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Competency)
            .WithMany()
            .HasForeignKey(e => e.CompetencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Period)
            .WithMany()
            .HasForeignKey(e => e.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
