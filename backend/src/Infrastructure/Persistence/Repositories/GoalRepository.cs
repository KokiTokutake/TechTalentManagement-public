using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.Goals;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class GoalRepository(AppDbContext db) : IGoalRepository
{
    public Task<EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct) =>
        db.EvaluationPeriods.FirstOrDefaultAsync(p => p.Id == periodId, ct);

    public async Task<IReadOnlyList<Goal>> GetByUserAndPeriodAsync(Guid userId, Guid periodId, CancellationToken ct) =>
        await db.Goals
            .Where(g => g.UserId == userId && g.EvaluationPeriodId == periodId)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync(ct);

    public Task<Goal?> FindAsync(Guid goalId, CancellationToken ct) =>
        db.Goals.FirstOrDefaultAsync(g => g.Id == goalId, ct);

    public async Task AddAsync(Goal goal, CancellationToken ct) =>
        await db.Goals.AddAsync(goal, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);

    public Task DeleteAsync(Goal goal, CancellationToken ct)
    {
        db.Goals.Remove(goal);
        return Task.CompletedTask;
    }
}
