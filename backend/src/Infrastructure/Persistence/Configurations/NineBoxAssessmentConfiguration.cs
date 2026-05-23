using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class NineBoxAssessmentConfiguration : IEntityTypeConfiguration<NineBoxAssessment>
{
    public void Configure(EntityTypeBuilder<NineBoxAssessment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.PerformanceRationale).HasMaxLength(1000);
        builder.Property(a => a.PotentialRationale).HasMaxLength(1000);
        builder.Property(a => a.PerformanceLevel).IsRequired();
        builder.Property(a => a.PotentialLevel).IsRequired();

        builder.HasOne(a => a.Talent)
               .WithMany()
               .HasForeignKey(a => a.TalentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Assessor)
               .WithMany()
               .HasForeignKey(a => a.AssessorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.EvaluationPeriod)
               .WithMany()
               .HasForeignKey(a => a.EvaluationPeriodId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.TalentId, a.EvaluationPeriodId }).IsUnique();
        builder.HasIndex(a => new { a.EvaluationPeriodId, a.PerformanceLevel, a.PotentialLevel });
    }
}
