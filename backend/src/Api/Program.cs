using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.CareerPlan;
using TechTalentManagement.Application.CommitteeSession;
using TechTalentManagement.Application.NineBox;
using TechTalentManagement.Application.DevelopmentPlan;
using TechTalentManagement.Application.KeyPerson;
using TechTalentManagement.Application.Goals;
using TechTalentManagement.Application.ManagerEvaluations;
using TechTalentManagement.Application.OneOnOne;
using TechTalentManagement.Application.SelfEvaluations;
using TechTalentManagement.Application.TalentProfile;
using TechTalentManagement.Application.MdcDecision;
using TechTalentManagement.Application.Wcm;
using TechTalentManagement.Application.CareerValues;
using TechTalentManagement.Application.GoalKpi;
using TechTalentManagement.Application.FeedbackMeeting;
using TechTalentManagement.Application.CompetencyMaster;
using TechTalentManagement.Application.CompetencyEvaluation;
using TechTalentManagement.Application.SkillGap;
using TechTalentManagement.Application.SkillLevelDefinition;
using TechTalentManagement.Application.SkillMaster;
using TechTalentManagement.Application.SkillMap;
using TechTalentManagement.Application.SkillMigration;
using TechTalentManagement.Application.Succession;
using TechTalentManagement.Application.ProjectAssignment;
using TechTalentManagement.Application.Notification;
using TechTalentManagement.Application.UserSkills;
using TechTalentManagement.Infrastructure.Persistence;
using TechTalentManagement.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<ISelfEvaluationRepository, SelfEvaluationRepository>();
builder.Services.AddScoped<ISelfEvaluationService, SelfEvaluationService>();
builder.Services.AddScoped<IManagerEvaluationRepository, ManagerEvaluationRepository>();
builder.Services.AddScoped<IManagerEvaluationService, ManagerEvaluationService>();
builder.Services.AddScoped<ISkillMasterRepository, SkillMasterRepository>();
builder.Services.AddScoped<ISkillMasterService, SkillMasterService>();
builder.Services.AddScoped<IUserSkillRepository, UserSkillRepository>();
builder.Services.AddScoped<IUserSkillService, UserSkillService>();
builder.Services.AddScoped<ISkillMapRepository, SkillMapRepository>();
builder.Services.AddScoped<ISkillMapService, SkillMapService>();
builder.Services.AddScoped<ICareerPlanRepository, CareerPlanRepository>();
builder.Services.AddScoped<ICareerPlanService, CareerPlanService>();
builder.Services.AddScoped<IKeyPersonRepository, KeyPersonRepository>();
builder.Services.AddScoped<IKeyPersonService, KeyPersonService>();
builder.Services.AddScoped<IDevelopmentPlanRepository, DevelopmentPlanRepository>();
builder.Services.AddScoped<IDevelopmentPlanService, DevelopmentPlanService>();
builder.Services.AddScoped<ICommitteeSessionRepository, CommitteeSessionRepository>();
builder.Services.AddScoped<ICommitteeSessionService, CommitteeSessionService>();
builder.Services.AddScoped<INineBoxRepository, NineBoxRepository>();
builder.Services.AddScoped<INineBoxService, NineBoxService>();
builder.Services.AddScoped<IOneOnOneRepository, OneOnOneRepository>();
builder.Services.AddScoped<IOneOnOneService, OneOnOneService>();
builder.Services.AddScoped<ITalentProfileRepository, TalentProfileRepository>();
builder.Services.AddScoped<ITalentProfileService, TalentProfileService>();
builder.Services.AddScoped<IMdcDecisionRepository, MdcDecisionRepository>();
builder.Services.AddScoped<IMdcDecisionService, MdcDecisionService>();
builder.Services.AddScoped<IWcmRepository, WcmRepository>();
builder.Services.AddScoped<IWcmService, WcmService>();
builder.Services.AddScoped<ICareerValuesRepository, CareerValuesRepository>();
builder.Services.AddScoped<ICareerValuesService, CareerValuesService>();
builder.Services.AddScoped<IGoalKpiRepository, GoalKpiRepository>();
builder.Services.AddScoped<IGoalKpiService, GoalKpiService>();
builder.Services.AddScoped<IFeedbackMeetingRepository, FeedbackMeetingRepository>();
builder.Services.AddScoped<IFeedbackMeetingService, FeedbackMeetingService>();
builder.Services.AddScoped<ISkillLevelDefinitionRepository, SkillLevelDefinitionRepository>();
builder.Services.AddScoped<ISkillLevelDefinitionService, SkillLevelDefinitionService>();
builder.Services.AddScoped<ICompetencyMasterRepository, CompetencyMasterRepository>();
builder.Services.AddScoped<ICompetencyMasterService, CompetencyMasterService>();
builder.Services.AddScoped<ICompetencyEvaluationRepository, CompetencyEvaluationRepository>();
builder.Services.AddScoped<ICompetencyEvaluationService, CompetencyEvaluationService>();
builder.Services.AddScoped<ISkillGapRepository, SkillGapRepository>();
builder.Services.AddScoped<ISkillGapService, SkillGapService>();
builder.Services.AddScoped<ISkillMigrationRepository, SkillMigrationRepository>();
builder.Services.AddScoped<ISkillMigrationService, SkillMigrationService>();
builder.Services.AddScoped<ICriticalPositionRepository, CriticalPositionRepository>();
builder.Services.AddScoped<ICriticalPositionService, CriticalPositionService>();
builder.Services.AddScoped<ISuccessionCandidateRepository, SuccessionCandidateRepository>();
builder.Services.AddScoped<ISuccessionCandidateService, SuccessionCandidateService>();
builder.Services.AddScoped<IProjectAssignmentRepository, ProjectAssignmentRepository>();
builder.Services.AddScoped<IProjectAssignmentService, ProjectAssignmentService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sql-server");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                builder.Configuration.GetValue<string>("AllowedOrigins")?.Split(',')
                ?? ["http://localhost:3000"])
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// 起動時にマイグレーション適用＆シードデータ投入
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db);
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
