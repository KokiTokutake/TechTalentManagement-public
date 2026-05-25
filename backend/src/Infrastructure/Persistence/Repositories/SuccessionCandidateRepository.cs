using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.Succession;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class SuccessionCandidateRepository(AppDbContext db) : ISuccessionCandidateRepository
{
    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FindAsync([userId], ct).AsTask();

    public Task<CriticalPosition?> FindPositionAsync(Guid positionId, CancellationToken ct) =>
        db.CriticalPositions.FindAsync([positionId], ct).AsTask();

    public Task<SuccessionCandidate?> FindCandidateAsync(Guid candidateId, CancellationToken ct) =>
        db.SuccessionCandidates
            .Include(c => c.Talent)
            .Include(c => c.Nominator)
            .Include(c => c.DevelopmentActionItems)
            .FirstOrDefaultAsync(c => c.Id == candidateId, ct);

    public Task<SuccessionCandidate?> FindCandidateByPositionAndTalentAsync(Guid positionId, Guid talentId, CancellationToken ct) =>
        db.SuccessionCandidates
            .FirstOrDefaultAsync(c => c.PositionId == positionId && c.TalentId == talentId, ct);

    public async Task<IReadOnlyList<SuccessionCandidate>> GetCandidatesByPositionAsync(Guid positionId, CancellationToken ct) =>
        await db.SuccessionCandidates
            .Where(c => c.PositionId == positionId)
            .Include(c => c.Talent)
            .Include(c => c.Nominator)
            .Include(c => c.DevelopmentActionItems)
            .OrderBy(c => c.Readiness)
            .ThenBy(c => c.NominatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SuccessionMapPositionProjection>> GetSuccessionMapAsync(CancellationToken ct)
    {
        var positions = await db.CriticalPositions
            .Where(p => p.IsActive)
            .Include(p => p.CurrentHolder)
            .Include(p => p.Candidates)
                .ThenInclude(c => c.Talent)
            .OrderBy(p => p.Importance)
            .ThenBy(p => p.PositionName)
            .ToListAsync(ct);

        return positions.Select(p =>
        {
            var readyNow = p.Candidates
                .Where(c => c.Readiness == ReadinessLevel.ReadyNow)
                .Select(c => new MapCandidateItem(c.Talent.Id, c.Talent.DisplayName, c.Id))
                .ToList();
            var readySoon = p.Candidates
                .Where(c => c.Readiness == ReadinessLevel.ReadySoon)
                .Select(c => new MapCandidateItem(c.Talent.Id, c.Talent.DisplayName, c.Id))
                .ToList();
            var readyMidTerm = p.Candidates
                .Where(c => c.Readiness == ReadinessLevel.ReadyMidTerm)
                .Select(c => new MapCandidateItem(c.Talent.Id, c.Talent.DisplayName, c.Id))
                .ToList();

            return new SuccessionMapPositionProjection(
                p.Id, p.PositionName, p.Importance,
                p.CurrentHolderId, p.CurrentHolder?.DisplayName,
                readyNow, readySoon, readyMidTerm);
        }).ToList();
    }

    public async Task<IReadOnlyList<NineBoxSuggestionProjection>> GetHighPotentialTalentsAsync(Guid periodId, CancellationToken ct) =>
        await db.NineBoxAssessments
            .Where(a => a.EvaluationPeriodId == periodId
                && (a.PotentialLevel == PotentialLevel.High || a.PotentialLevel == PotentialLevel.Mid))
            .Include(a => a.Talent)
            .Select(a => new NineBoxSuggestionProjection(
                a.TalentId, a.Talent.DisplayName, a.PotentialLevel, a.PerformanceLevel))
            .ToListAsync(ct);

    public async Task<HashSet<Guid>> GetExistingCandidateTalentIdsAsync(Guid positionId, CancellationToken ct)
    {
        var ids = await db.SuccessionCandidates
            .Where(c => c.PositionId == positionId)
            .Select(c => c.TalentId)
            .ToListAsync(ct);
        return [.. ids];
    }

    public Task<SuccessionDevelopmentAction?> FindDevelopmentActionAsync(Guid actionId, CancellationToken ct) =>
        db.SuccessionDevelopmentActions.FindAsync([actionId], ct).AsTask();

    public async Task AddCandidateAsync(SuccessionCandidate candidate, CancellationToken ct) =>
        await db.SuccessionCandidates.AddAsync(candidate, ct);

    public async Task AddDevelopmentActionAsync(SuccessionDevelopmentAction action, CancellationToken ct) =>
        await db.SuccessionDevelopmentActions.AddAsync(action, ct);

    public Task RemoveCandidateAsync(SuccessionCandidate candidate, CancellationToken ct)
    {
        db.SuccessionCandidates.Remove(candidate);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
