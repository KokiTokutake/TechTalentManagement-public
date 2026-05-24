using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence;

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
    public DbSet<WillCanMustEntry> WillCanMustEntries => Set<WillCanMustEntry>();
    public DbSet<CareerValues> CareerValues => Set<CareerValues>();
    public DbSet<GoalKpi> GoalKpis => Set<GoalKpi>();
    public DbSet<GoalCheckin> GoalCheckins => Set<GoalCheckin>();
    public DbSet<FeedbackMeeting> FeedbackMeetings => Set<FeedbackMeeting>();
    public DbSet<SkillLevelDefinition> SkillLevelDefinitions => Set<SkillLevelDefinition>();
    public DbSet<CompetencyMaster> CompetencyMasters => Set<CompetencyMaster>();
    public DbSet<CompetencyEvaluation> CompetencyEvaluations => Set<CompetencyEvaluation>();
    public DbSet<SkillLevelMigrationLog> SkillLevelMigrationLogs => Set<SkillLevelMigrationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
