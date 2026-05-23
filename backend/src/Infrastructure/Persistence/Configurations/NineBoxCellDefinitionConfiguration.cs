using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class NineBoxCellDefinitionConfiguration : IEntityTypeConfiguration<NineBoxCellDefinition>
{
    public void Configure(EntityTypeBuilder<NineBoxCellDefinition> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Label).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500).IsRequired();
        builder.Property(c => c.Color).HasMaxLength(20).IsRequired();
        builder.Property(c => c.PerformanceLevel).IsRequired();
        builder.Property(c => c.PotentialLevel).IsRequired();

        builder.HasIndex(c => new { c.PerformanceLevel, c.PotentialLevel }).IsUnique();
    }
}
