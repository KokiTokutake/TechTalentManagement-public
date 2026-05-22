using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Configurations;

public class TalentManagerCommentConfiguration : IEntityTypeConfiguration<TalentManagerComment>
{
    public void Configure(EntityTypeBuilder<TalentManagerComment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CurrentChallenges).HasMaxLength(2000);
        builder.Property(c => c.DevelopmentPolicy).HasMaxLength(2000);
        builder.Property(c => c.ExpectedRole).HasMaxLength(1000);

        builder.HasIndex(c => new { c.TalentId, c.PeriodId }).IsUnique();
        builder.HasIndex(c => c.ManagerId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.TalentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EvaluationPeriod>()
            .WithMany()
            .HasForeignKey(c => c.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
