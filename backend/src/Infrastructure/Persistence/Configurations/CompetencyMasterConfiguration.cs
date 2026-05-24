using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class CompetencyMasterConfiguration : IEntityTypeConfiguration<CompetencyMaster>
{
    public void Configure(EntityTypeBuilder<CompetencyMaster> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);

        builder.HasIndex(c => new { c.Category, c.Name }).IsUnique();
    }
}
