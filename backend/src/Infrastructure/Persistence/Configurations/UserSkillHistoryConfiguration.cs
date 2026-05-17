using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Configurations;

public class UserSkillHistoryConfiguration : IEntityTypeConfiguration<UserSkillHistory>
{
    public void Configure(EntityTypeBuilder<UserSkillHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Status).HasConversion<string>().IsRequired();

        builder.HasOne(h => h.UserSkill)
            .WithMany()
            .HasForeignKey(h => h.UserSkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
