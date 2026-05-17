using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Configurations;

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
