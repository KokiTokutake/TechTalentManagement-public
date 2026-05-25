using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class SuccessionDevelopmentActionConfiguration : IEntityTypeConfiguration<SuccessionDevelopmentAction>
{
    public void Configure(EntityTypeBuilder<SuccessionDevelopmentAction> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).HasMaxLength(500).IsRequired();

        builder.HasIndex(a => new { a.CandidateId, a.Status });

        builder.HasOne(a => a.Candidate)
            .WithMany(c => c.DevelopmentActionItems)
            .HasForeignKey(a => a.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
