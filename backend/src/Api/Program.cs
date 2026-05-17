using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Application.Goals;
using TeckTalentManagement.Application.ManagerEvaluations;
using TeckTalentManagement.Application.SelfEvaluations;
using TeckTalentManagement.Application.SkillMaster;
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
