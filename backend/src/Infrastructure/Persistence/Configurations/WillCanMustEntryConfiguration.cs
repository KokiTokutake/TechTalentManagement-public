using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class WillCanMustEntryConfiguration : IEntityTypeConfiguration<WillCanMustEntry>
{
    public void Configure(EntityTypeBuilder<WillCanMustEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Content).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Category).IsRequired();

        builder.HasIndex(e => new { e.TalentId, e.PeriodId, e.Category });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.TalentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EvaluationPeriod>()
            .WithMany()
            .HasForeignKey(e => e.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SkillItem>()
            .WithMany()
            .HasForeignKey(e => e.LinkedSkillItemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
