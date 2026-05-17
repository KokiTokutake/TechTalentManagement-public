using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Configurations;

public class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
{
    public void Configure(EntityTypeBuilder<UserSkill> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Level).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().IsRequired();
        builder.Property(s => s.Comment).HasMaxLength(1000);
        builder.Property(s => s.RejectedReason).HasMaxLength(1000);

        builder.HasIndex(s => new { s.UserId, s.SkillItemId }).IsUnique();

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.SkillItem)
            .WithMany()
            .HasForeignKey(s => s.SkillItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
