using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Configurations;

public class MdcDecisionConfiguration : IEntityTypeConfiguration<MdcDecision>
{
    public void Configure(EntityTypeBuilder<MdcDecision> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Content).IsRequired().HasMaxLength(2000);
        builder.Property(d => d.DecisionType).IsRequired();

        builder.HasIndex(d => d.SessionId);

        builder.HasOne<CommitteeSession>()
            .WithMany()
            .HasForeignKey(d => d.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Talents)
            .WithOne()
            .HasForeignKey(t => t.DecisionId)
            .OnDelete(DeleteBehavior.Cascade);

        // SQL Server では CommitteeSession→MdcDecision(Cascade) と CommitteeSession→MdcActionItem(Cascade) の
        // 二重カスケードパスが禁止されるため ClientSetNull（アプリ側で null 化）を使用
        builder.HasMany(d => d.ActionItems)
            .WithOne()
            .HasForeignKey(a => a.DecisionId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}

public class MdcDecisionTalentConfiguration : IEntityTypeConfiguration<MdcDecisionTalent>
{
    public void Configure(EntityTypeBuilder<MdcDecisionTalent> builder)
    {
        builder.HasKey(t => new { t.DecisionId, t.TalentId });
        builder.HasIndex(t => t.TalentId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.TalentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
