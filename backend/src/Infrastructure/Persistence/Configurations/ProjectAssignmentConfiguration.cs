using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class ProjectAssignmentConfiguration : IEntityTypeConfiguration<ProjectAssignment>
{
    public void Configure(EntityTypeBuilder<ProjectAssignment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ProjectName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Role).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.ManagerComment).HasMaxLength(500);

        builder.HasIndex(e => new { e.TalentId, e.StartDate });
        builder.HasIndex(e => new { e.TalentId, e.EndDate });

        builder.HasOne(e => e.Talent)
            .WithMany()
            .HasForeignKey(e => e.TalentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ManagerCommentedBy)
            .WithMany()
            .HasForeignKey(e => e.ManagerCommentedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
