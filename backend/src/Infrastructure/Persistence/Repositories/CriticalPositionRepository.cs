using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.Succession;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class CriticalPositionRepository(AppDbContext db) : ICriticalPositionRepository
{
    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FindAsync([userId], ct).AsTask();

    public Task<CriticalPosition?> FindByIdAsync(Guid id, CancellationToken ct) =>
        db.CriticalPositions
            .Include(p => p.CurrentHolder)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<CriticalPositionListProjection>> GetListAsync(bool includeInactive, CancellationToken ct)
    {
        var query = db.CriticalPositions
            .Include(p => p.CurrentHolder)
            .Include(p => p.Candidates)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        var positions = await query
            .OrderBy(p => p.Importance)
            .ThenBy(p => p.PositionName)
            .ToListAsync(ct);

        return positions.Select(p => new CriticalPositionListProjection(
            p.Id,
            p.PositionName,
            p.CurrentHolderId,
            p.CurrentHolder?.DisplayName,
            p.Importance,
            p.RequiredSkills,
            p.IsActive,
            CandidateCount: p.Candidates.Count,
            HasReadyNowCandidate: p.Candidates.Any(c => c.Readiness == ReadinessLevel.ReadyNow),
            p.CreatedAt,
            p.UpdatedAt
        )).ToList();
    }

    public async Task AddAsync(CriticalPosition position, CancellationToken ct) =>
        await db.CriticalPositions.AddAsync(position, ct);

    public async Task<IReadOnlyList<TalentCandidacyItem>> GetCandidaciesByTalentIdAsync(Guid talentId, CancellationToken ct) =>
        await db.SuccessionCandidates
            .Where(c => c.TalentId == talentId)
            .Include(c => c.Position)
            .OrderByDescending(c => c.NominatedAt)
            .Select(c => new TalentCandidacyItem(
                c.PositionId,
                c.Position.PositionName,
                c.Position.Importance.ToString(),
                c.Readiness.ToString(),
                c.NominatedAt
            ))
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
