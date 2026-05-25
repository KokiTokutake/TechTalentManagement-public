using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class CriticalPositionConfiguration : IEntityTypeConfiguration<CriticalPosition>
{
    public void Configure(EntityTypeBuilder<CriticalPosition> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PositionName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.RequiredSkills).HasMaxLength(2000);

        builder.HasIndex(p => new { p.Importance, p.IsActive });
        builder.HasIndex(p => p.CurrentHolderId);

        builder.HasOne(p => p.CurrentHolder)
            .WithMany()
            .HasForeignKey(p => p.CurrentHolderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
