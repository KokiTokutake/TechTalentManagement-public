using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class SkillItemConfiguration : IEntityTypeConfiguration<SkillItem>
{
    public void Configure(EntityTypeBuilder<SkillItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(i => new { i.CategoryId, i.Name }).IsUnique();
    }
}
