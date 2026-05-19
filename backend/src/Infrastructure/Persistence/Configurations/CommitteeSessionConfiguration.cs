using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Configurations;

public class CommitteeSessionConfiguration : IEntityTypeConfiguration<CommitteeSession>
{
    public void Configure(EntityTypeBuilder<CommitteeSession> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Minutes).IsRequired().HasMaxLength(10000);

        builder.HasOne<Domain.Entities.User>()
            .WithMany()
            .HasForeignKey(s => s.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Participants)
            .WithOne()
            .HasForeignKey(p => p.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.KeyPersons)
            .WithOne()
            .HasForeignKey(k => k.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CommitteeSessionParticipantConfiguration : IEntityTypeConfiguration<CommitteeSessionParticipant>
{
    public void Configure(EntityTypeBuilder<CommitteeSessionParticipant> builder)
    {
        builder.HasKey(p => new { p.SessionId, p.UserId });

        builder.HasOne<Domain.Entities.User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CommitteeSessionKeyPersonConfiguration : IEntityTypeConfiguration<CommitteeSessionKeyPerson>
{
    public void Configure(EntityTypeBuilder<CommitteeSessionKeyPerson> builder)
    {
        builder.HasKey(k => new { k.SessionId, k.KeyPersonId });

        builder.HasOne<KeyPerson>()
            .WithMany()
            .HasForeignKey(k => k.KeyPersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
