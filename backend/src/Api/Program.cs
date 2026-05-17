using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Application.Goals;
using TeckTalentManagement.Application.ManagerEvaluations;
using TeckTalentManagement.Application.SelfEvaluations;
using TeckTalentManagement.Infrastructure.Persistence;
using TeckTalentManagement.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<ISelfEvaluationRepository, SelfEvaluationRepository>();
builder.Services.AddScoped<ISelfEvaluationService, SelfEvaluationService>();
builder.Services.AddScoped<IManagerEvaluationRepository, ManagerEvaluationRepository>();
builder.Services.AddScoped<IManagerEvaluationService, ManagerEvaluationService>();

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

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
