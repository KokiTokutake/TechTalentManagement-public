using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class FeedbackMeetingConfiguration : IEntityTypeConfiguration<FeedbackMeeting>
{
    public void Configure(EntityTypeBuilder<FeedbackMeeting> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.OverallComment).HasMaxLength(3000);
        builder.Property(f => f.NextPeriodExpectation).HasMaxLength(2000);
        builder.Property(f => f.TalentReactionNote).HasMaxLength(1000);
        builder.Property(f => f.IsDisclosed).IsRequired().HasDefaultValue(false);

        builder.HasIndex(f => f.SelfEvaluationId).IsUnique();
        builder.HasIndex(f => new { f.TalentId, f.IsDisclosed });

        builder.HasOne(f => f.SelfEvaluation)
               .WithOne(s => s.FeedbackMeeting)
               .HasForeignKey<FeedbackMeeting>(f => f.SelfEvaluationId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Talent)
               .WithMany()
               .HasForeignKey(f => f.TalentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Manager)
               .WithMany()
               .HasForeignKey(f => f.ManagerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
