using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class MeetingActionItemConfiguration : IEntityTypeConfiguration<MeetingActionItem>
{
    public void Configure(EntityTypeBuilder<MeetingActionItem> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Description).HasMaxLength(500).IsRequired();
        builder.Property(a => a.Status).IsRequired();

        builder.HasIndex(a => new { a.MeetingId, a.Status });

        // OneOnOneMeeting の論理削除フィルターと整合させる
        builder.HasQueryFilter(a => !a.Meeting!.IsDeleted);
    }
}
