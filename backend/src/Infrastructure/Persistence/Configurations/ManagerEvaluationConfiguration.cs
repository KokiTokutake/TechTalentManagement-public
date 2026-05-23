using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class ManagerEvaluationConfiguration : IEntityTypeConfiguration<ManagerEvaluation>
{
    public void Configure(EntityTypeBuilder<ManagerEvaluation> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.OverallScore).IsRequired();
        builder.Property(m => m.FeedbackComment).HasMaxLength(4000);

        builder.HasOne(m => m.Manager)
               .WithMany(u => u.ManagerEvaluations)
               .HasForeignKey(m => m.ManagerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
