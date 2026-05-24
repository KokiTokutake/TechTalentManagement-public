using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class SkillLevelDefinitionConfiguration : IEntityTypeConfiguration<SkillLevelDefinition>
{
    public void Configure(EntityTypeBuilder<SkillLevelDefinition> builder)
    {
        builder.HasKey(d => new { d.SkillItemId, d.Level });
        builder.Property(d => d.Description).HasMaxLength(1000).IsRequired();
        builder.Property(d => d.Examples).HasMaxLength(2000);

        builder.HasOne(d => d.SkillItem)
            .WithMany()
            .HasForeignKey(d => d.SkillItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
