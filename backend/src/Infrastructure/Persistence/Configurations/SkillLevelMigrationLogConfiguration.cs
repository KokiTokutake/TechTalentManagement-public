using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class SkillLevelMigrationLogConfiguration : IEntityTypeConfiguration<SkillLevelMigrationLog>
{
    public void Configure(EntityTypeBuilder<SkillLevelMigrationLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.ConversionRuleJson).HasMaxLength(500).IsRequired();

        builder.HasOne(l => l.Executor)
            .WithMany()
            .HasForeignKey(l => l.ExecutedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
