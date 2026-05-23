using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Configurations;

public class CareerValuesConfiguration : IEntityTypeConfiguration<CareerValues>
{
    public void Configure(EntityTypeBuilder<CareerValues> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.ImportantThings).HasMaxLength(1000);
        builder.Property(v => v.MotivationSituations).HasMaxLength(1000);
        builder.Property(v => v.AvoidSituations).HasMaxLength(1000);

        builder.HasIndex(v => new { v.TalentId, v.PeriodId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(v => v.TalentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<EvaluationPeriod>()
            .WithMany()
            .HasForeignKey(v => v.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
