using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.CompetencyEvaluation;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class CompetencyEvaluationRepository(AppDbContext db) : ICompetencyEvaluationRepository
{
    public Task<UserRole?> GetUserRoleAsync(Guid userId, CancellationToken ct) =>
        db.Users.Where(u => u.Id == userId).Select(u => (UserRole?)u.Role).FirstOrDefaultAsync(ct);

    public Task<bool> IsManagedByAsync(Guid managerId, Guid talentId, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Id == talentId && u.ManagerId == managerId, ct);

    public Task<bool> PeriodExistsAsync(Guid periodId, CancellationToken ct) =>
        db.EvaluationPeriods.AnyAsync(p => p.Id == periodId, ct);

    public Task<bool> CompetencyExistsAsync(Guid competencyId, CancellationToken ct) =>
        db.CompetencyMasters.AnyAsync(c => c.Id == competencyId && c.IsActive, ct);

    public Task<CompetencyEvaluation?> FindEvaluationAsync(
        Guid talentId, Guid evaluatorId, Guid competencyId, Guid periodId,
        CompetencyEvaluatorType evaluatorType, CancellationToken ct) =>
        db.CompetencyEvaluations.FirstOrDefaultAsync(e =>
            e.TalentId == talentId &&
            e.EvaluatorId == evaluatorId &&
            e.CompetencyId == competencyId &&
            e.PeriodId == periodId &&
            e.EvaluatorType == evaluatorType, ct);

    public async Task AddEvaluationAsync(CompetencyEvaluation evaluation, CancellationToken ct) =>
        await db.CompetencyEvaluations.AddAsync(evaluation, ct);

    public async Task<IReadOnlyList<CompetencyEvaluationWithMaster>> GetEvaluationsWithMasterAsync(
        Guid talentId, Guid periodId, CancellationToken ct) =>
        await db.CompetencyEvaluations
            .Include(e => e.Competency)
            .Where(e => e.TalentId == talentId && e.PeriodId == periodId)
            .Select(e => new CompetencyEvaluationWithMaster(
                e.CompetencyId,
                e.Competency!.Name,
                e.Competency.Category.ToString(),
                e.EvaluatorType,
                e.Level,
                e.Comment,
                e.UpdatedAt))
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
