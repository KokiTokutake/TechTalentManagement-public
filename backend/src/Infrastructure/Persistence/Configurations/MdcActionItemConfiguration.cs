using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Configurations;

public class MdcActionItemConfiguration : IEntityTypeConfiguration<MdcActionItem>
{
    public void Configure(EntityTypeBuilder<MdcActionItem> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(500);
        builder.Property(a => a.Status).IsRequired();

        builder.HasIndex(a => a.SessionId);
        builder.HasIndex(a => new { a.AssigneeId, a.Status });
        builder.HasIndex(a => a.DecisionId);

        builder.HasOne<CommitteeSession>()
            .WithMany()
            .HasForeignKey(a => a.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        // DecisionId の FK は MdcDecisionConfiguration 側の HasMany で設定済み
    }
}
