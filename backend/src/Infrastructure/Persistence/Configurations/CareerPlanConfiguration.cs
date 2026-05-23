using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class CareerPlanConfiguration : IEntityTypeConfiguration<CareerPlan>
{
    public void Configure(EntityTypeBuilder<CareerPlan> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.LongTermGoal).IsRequired().HasMaxLength(1000);
        builder.Property(p => p.MediumTermGoal).IsRequired().HasMaxLength(1000);
        builder.Property(p => p.CurrentFocus).IsRequired().HasMaxLength(1000);

        builder.HasIndex(p => new { p.UserId, p.PeriodYear }).IsUnique();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Comments)
            .WithOne(c => c.CareerPlan)
            .HasForeignKey(c => c.CareerPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CareerPlanCommentConfiguration : IEntityTypeConfiguration<CareerPlanComment>
{
    public void Configure(EntityTypeBuilder<CareerPlanComment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Content).IsRequired().HasMaxLength(1000);

        builder.HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
