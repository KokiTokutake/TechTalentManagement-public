using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class KeyPersonConfiguration : IEntityTypeConfiguration<KeyPerson>
{
    public void Configure(EntityTypeBuilder<KeyPerson> builder)
    {
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Reason).HasMaxLength(1000);

        builder.HasIndex(k => k.TalentId)
            .HasFilter("[IsActive] = 1");

        builder.HasOne<Domain.Entities.User>()
            .WithMany()
            .HasForeignKey(k => k.TalentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Domain.Entities.User>()
            .WithMany()
            .HasForeignKey(k => k.DesignatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(k => k.DevelopmentPlans)
            .WithOne()
            .HasForeignKey(d => d.KeyPersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class DevelopmentPlanConfiguration : IEntityTypeConfiguration<DevelopmentPlan>
{
    public void Configure(EntityTypeBuilder<DevelopmentPlan> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Challenge).IsRequired().HasMaxLength(2000);
        builder.Property(d => d.Goal).IsRequired().HasMaxLength(2000);
        builder.Property(d => d.SupportAction).IsRequired().HasMaxLength(2000);

        builder.HasOne<Domain.Entities.User>()
            .WithMany()
            .HasForeignKey(d => d.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
