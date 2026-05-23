using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class OneOnOneMeetingConfiguration : IEntityTypeConfiguration<OneOnOneMeeting>
{
    public void Configure(EntityTypeBuilder<OneOnOneMeeting> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.WillTopics).HasMaxLength(2000);
        builder.Property(m => m.CanTopics).HasMaxLength(2000);
        builder.Property(m => m.MustTopics).HasMaxLength(2000);
        builder.Property(m => m.CurrentSituation).HasMaxLength(2000);
        builder.Property(m => m.FreeText).HasMaxLength(5000);
        builder.Property(m => m.Format).IsRequired();
        builder.Property(m => m.ShareWithMdc).IsRequired();
        builder.Property(m => m.IsDeleted).IsRequired();

        builder.HasOne(m => m.Talent)
               .WithMany()
               .HasForeignKey(m => m.TalentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Manager)
               .WithMany()
               .HasForeignKey(m => m.ManagerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.ActionItems)
               .WithOne(a => a.Meeting)
               .HasForeignKey(a => a.MeetingId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.TalentId, m.ConductedAt });
        builder.HasIndex(m => new { m.ManagerId, m.TalentId });
        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
