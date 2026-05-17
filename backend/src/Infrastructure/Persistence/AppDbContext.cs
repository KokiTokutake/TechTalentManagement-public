using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<EvaluationPeriod> EvaluationPeriods => Set<EvaluationPeriod>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<SelfEvaluation> SelfEvaluations => Set<SelfEvaluation>();
    public DbSet<SelfEvaluationItem> SelfEvaluationItems => Set<SelfEvaluationItem>();
    public DbSet<ManagerEvaluation> ManagerEvaluations => Set<ManagerEvaluation>();
    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();
    public DbSet<SkillItem> SkillItems => Set<SkillItem>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<UserSkillHistory> UserSkillHistories => Set<UserSkillHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
