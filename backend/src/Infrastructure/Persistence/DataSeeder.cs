using TeckTalentManagement.Domain.Entities;
using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var manager = User.Create("dev-manager", "山田 一郎", "yamada@example.com", UserRole.Manager);
        var talent1 = User.Create("dev-talent1", "田中 太郎", "tanaka@example.com", UserRole.Talent, manager.Id);
        var talent2 = User.Create("dev-talent2", "佐藤 花子", "sato@example.com", UserRole.Talent, manager.Id);

        db.Users.AddRange(manager, talent1, talent2);

        if (!db.EvaluationPeriods.Any())
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var goalOpenPeriod = EvaluationPeriod.Create(
                DateTime.UtcNow.Year, $"{DateTime.UtcNow.Year}年度考課",
                today.AddDays(-30), today.AddDays(30),    // 目標登録期間：オープン
                today.AddDays(60), today.AddDays(120),
                today.AddDays(150));
            db.EvaluationPeriods.Add(goalOpenPeriod);
        }

        await db.SaveChangesAsync();
    }
}
