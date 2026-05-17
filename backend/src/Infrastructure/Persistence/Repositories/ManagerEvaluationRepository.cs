using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Application.ManagerEvaluations;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Repositories;

public class ManagerEvaluationRepository(AppDbContext db) : IManagerEvaluationRepository
{
    public Task<SelfEvaluation?> FindSelfEvaluationAsync(Guid selfEvaluationId, CancellationToken ct) =>
        db.SelfEvaluations.FirstOrDefaultAsync(e => e.Id == selfEvaluationId, ct);

    public Task<ManagerEvaluation?> FindAsync(Guid selfEvaluationId, CancellationToken ct) =>
        db.ManagerEvaluations.FirstOrDefaultAsync(e => e.SelfEvaluationId == selfEvaluationId, ct);

    public async Task AddAsync(ManagerEvaluation evaluation, CancellationToken ct) =>
        await db.ManagerEvaluations.AddAsync(evaluation, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
