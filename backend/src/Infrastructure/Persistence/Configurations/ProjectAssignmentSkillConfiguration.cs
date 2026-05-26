using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class ProjectAssignmentSkillConfiguration : IEntityTypeConfiguration<ProjectAssignmentSkill>
{
    public void Configure(EntityTypeBuilder<ProjectAssignmentSkill> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.AssignmentId, e.SkillItemId }).IsUnique();

        builder.HasOne(e => e.Assignment)
            .WithMany(a => a.Skills)
            .HasForeignKey(e => e.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SkillItem)
            .WithMany()
            .HasForeignKey(e => e.SkillItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
