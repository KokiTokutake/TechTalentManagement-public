using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.SelfEvaluations;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class SelfEvaluationRepository(AppDbContext db) : ISelfEvaluationRepository
{
    public Task<EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct) =>
        db.EvaluationPeriods.FirstOrDefaultAsync(p => p.Id == periodId, ct);

    public async Task<IReadOnlyList<Goal>> GetGoalsByUserAndPeriodAsync(Guid userId, Guid periodId, CancellationToken ct) =>
        await db.Goals
            .Where(g => g.UserId == userId && g.EvaluationPeriodId == periodId)
            .ToListAsync(ct);

    public Task<SelfEvaluation?> FindAsync(Guid userId, Guid periodId, CancellationToken ct) =>
        db.SelfEvaluations
            .Include(e => e.Items)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.EvaluationPeriodId == periodId, ct);

    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FindAsync([userId], ct).AsTask();

    public async Task AddAsync(SelfEvaluation evaluation, CancellationToken ct) =>
        await db.SelfEvaluations.AddAsync(evaluation, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
