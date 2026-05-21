using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Application.CareerPlan;
using TeckTalentManagement.Application.CommitteeSession;
using TeckTalentManagement.Application.NineBox;
using TeckTalentManagement.Application.DevelopmentPlan;
using TeckTalentManagement.Application.KeyPerson;
using TeckTalentManagement.Application.Goals;
using TeckTalentManagement.Application.ManagerEvaluations;
using TeckTalentManagement.Application.OneOnOne;
using TeckTalentManagement.Application.SelfEvaluations;
using TeckTalentManagement.Application.SkillMaster;
using TeckTalentManagement.Application.SkillMap;
using TeckTalentManagement.Application.UserSkills;
using TeckTalentManagement.Infrastructure.Persistence;
using TeckTalentManagement.Infrastructure.Persistence.Repositories;

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
