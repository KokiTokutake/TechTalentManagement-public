using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.GoalKpi;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class GoalKpiRepository(AppDbContext db) : IGoalKpiRepository
{
    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task<Goal?> FindGoalAsync(Guid goalId, CancellationToken ct) =>
        db.Goals.FirstOrDefaultAsync(g => g.Id == goalId, ct);

    public Task<EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct) =>
        db.EvaluationPeriods.FirstOrDefaultAsync(p => p.Id == periodId, ct);

    public Task<GoalKpi?> FindKpiByGoalAsync(Guid goalId, CancellationToken ct) =>
        db.GoalKpis.FirstOrDefaultAsync(k => k.GoalId == goalId, ct);

    public async Task<IReadOnlyList<GoalCheckin>> GetCheckinsAsync(Guid goalId, CancellationToken ct) =>
        await db.GoalCheckins
            .Where(c => c.GoalId == goalId)
            .OrderByDescending(c => c.CheckedInAt)
            .ToListAsync(ct);

    public async Task AddKpiAsync(GoalKpi kpi, CancellationToken ct) =>
        await db.GoalKpis.AddAsync(kpi, ct);

    public async Task AddCheckinAsync(GoalCheckin checkin, CancellationToken ct) =>
        await db.GoalCheckins.AddAsync(checkin, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
