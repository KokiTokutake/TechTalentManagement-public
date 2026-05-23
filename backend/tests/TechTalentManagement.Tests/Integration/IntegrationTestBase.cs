using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Infrastructure.Persistence;

namespace TechTalentManagement.Tests.Integration;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly AppDbContext Db;

    protected IntegrationTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Db = new AppDbContext(options);
    }

    protected static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    protected static TechTalentManagement.Domain.Entities.EvaluationPeriod CreateOpenGoalPeriod() =>
        TechTalentManagement.Domain.Entities.EvaluationPeriod.Create(
            DateTime.UtcNow.Year, "テスト期間",
            Today.AddDays(-10), Today.AddDays(10),
            Today.AddDays(30), Today.AddDays(60),
            Today.AddDays(90));

    protected static TechTalentManagement.Domain.Entities.EvaluationPeriod CreateOpenEvalPeriod() =>
        TechTalentManagement.Domain.Entities.EvaluationPeriod.Create(
            DateTime.UtcNow.Year, "評価期間",
            Today.AddDays(-60), Today.AddDays(-30),
            Today.AddDays(-10), Today.AddDays(10),
            Today.AddDays(30));

    protected static TechTalentManagement.Domain.Entities.EvaluationPeriod CreateClosedPeriod() =>
        TechTalentManagement.Domain.Entities.EvaluationPeriod.Create(
            DateTime.UtcNow.Year - 1, "終了期間",
            Today.AddDays(-120), Today.AddDays(-90),
            Today.AddDays(-60), Today.AddDays(-30),
            Today.AddDays(-10));

    public void Dispose() => Db.Dispose();
}
