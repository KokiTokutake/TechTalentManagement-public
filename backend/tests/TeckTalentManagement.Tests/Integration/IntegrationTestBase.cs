using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Infrastructure.Persistence;

namespace TeckTalentManagement.Tests.Integration;

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

    protected static TeckTalentManagement.Domain.Entities.EvaluationPeriod CreateOpenGoalPeriod() =>
        TeckTalentManagement.Domain.Entities.EvaluationPeriod.Create(
            DateTime.UtcNow.Year, "テスト期間",
            Today.AddDays(-10), Today.AddDays(10),
            Today.AddDays(30), Today.AddDays(60),
            Today.AddDays(90));

    protected static TeckTalentManagement.Domain.Entities.EvaluationPeriod CreateOpenEvalPeriod() =>
        TeckTalentManagement.Domain.Entities.EvaluationPeriod.Create(
            DateTime.UtcNow.Year, "評価期間",
            Today.AddDays(-60), Today.AddDays(-30),
            Today.AddDays(-10), Today.AddDays(10),
            Today.AddDays(30));

    protected static TeckTalentManagement.Domain.Entities.EvaluationPeriod CreateClosedPeriod() =>
        TeckTalentManagement.Domain.Entities.EvaluationPeriod.Create(
            DateTime.UtcNow.Year - 1, "終了期間",
            Today.AddDays(-120), Today.AddDays(-90),
            Today.AddDays(-60), Today.AddDays(-30),
            Today.AddDays(-10));

    public void Dispose() => Db.Dispose();
}
