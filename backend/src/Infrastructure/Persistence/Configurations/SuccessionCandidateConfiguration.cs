using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class SuccessionCandidateConfiguration : IEntityTypeConfiguration<SuccessionCandidate>
{
    public void Configure(EntityTypeBuilder<SuccessionCandidate> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.DevelopmentGap).HasMaxLength(1000);
        builder.Property(c => c.DevelopmentActions).HasMaxLength(2000);

        builder.HasIndex(c => new { c.PositionId, c.TalentId }).IsUnique();
        builder.HasIndex(c => new { c.PositionId, c.Readiness });
        builder.HasIndex(c => c.TalentId);

        builder.HasOne(c => c.Position)
            .WithMany(p => p.Candidates)
            .HasForeignKey(c => c.PositionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Talent)
            .WithMany()
            .HasForeignKey(c => c.TalentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Nominator)
            .WithMany()
            .HasForeignKey(c => c.NominatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
