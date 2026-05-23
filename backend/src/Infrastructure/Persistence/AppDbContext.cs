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
    public DbSet<CareerPlan> CareerPlans => Set<CareerPlan>();
    public DbSet<CareerPlanComment> CareerPlanComments => Set<CareerPlanComment>();
    public DbSet<KeyPerson> KeyPersons => Set<KeyPerson>();
    public DbSet<DevelopmentPlan> DevelopmentPlans => Set<DevelopmentPlan>();
    public DbSet<CommitteeSession> CommitteeSessions => Set<CommitteeSession>();
    public DbSet<CommitteeSessionParticipant> CommitteeSessionParticipants => Set<CommitteeSessionParticipant>();
    public DbSet<CommitteeSessionKeyPerson> CommitteeSessionKeyPersons => Set<CommitteeSessionKeyPerson>();
    public DbSet<NineBoxAssessment> NineBoxAssessments => Set<NineBoxAssessment>();
    public DbSet<NineBoxCellDefinition> NineBoxCellDefinitions => Set<NineBoxCellDefinition>();
    public DbSet<OneOnOneMeeting> OneOnOneMeetings => Set<OneOnOneMeeting>();
    public DbSet<MeetingActionItem> MeetingActionItems => Set<MeetingActionItem>();
    public DbSet<TalentManagerComment> TalentManagerComments => Set<TalentManagerComment>();
    public DbSet<MdcDecision> MdcDecisions => Set<MdcDecision>();
    public DbSet<MdcDecisionTalent> MdcDecisionTalents => Set<MdcDecisionTalent>();
    public DbSet<MdcActionItem> MdcActionItems => Set<MdcActionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
