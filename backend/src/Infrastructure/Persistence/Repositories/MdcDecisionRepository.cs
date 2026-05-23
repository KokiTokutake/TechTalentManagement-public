using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Application.MdcDecision;
using TeckTalentManagement.Domain.Enums;
using DomainEntities = TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Repositories;

public class MdcDecisionRepository(AppDbContext db) : IMdcDecisionRepository
{
    public Task<DomainEntities.User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FindAsync([userId], ct).AsTask();

    public Task<DomainEntities.CommitteeSession?> FindSessionAsync(Guid sessionId, CancellationToken ct) =>
        db.CommitteeSessions.FindAsync([sessionId], ct).AsTask();

    public async Task<IReadOnlyList<DomainEntities.MdcDecision>> GetDecisionsBySessionAsync(Guid sessionId, CancellationToken ct) =>
        await db.MdcDecisions
            .Include(d => d.Talents)
            .Include(d => d.ActionItems)
            .Where(d => d.SessionId == sessionId)
            .OrderBy(d => d.DecidedAt)
            .ToListAsync(ct);

    public Task<DomainEntities.MdcDecision?> FindDecisionAsync(Guid id, CancellationToken ct) =>
        db.MdcDecisions
            .Include(d => d.Talents)
            .Include(d => d.ActionItems)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task AddDecisionAsync(DomainEntities.MdcDecision decision, CancellationToken ct) =>
        await db.MdcDecisions.AddAsync(decision, ct);

    public async Task<IReadOnlyList<DomainEntities.MdcActionItem>> GetActionItemsBySessionAsync(Guid sessionId, MdcActionItemStatus? status, CancellationToken ct)
    {
        var query = db.MdcActionItems.Where(a => a.SessionId == sessionId);
        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);
        return await query.OrderBy(a => a.DueDate).ToListAsync(ct);
    }

    public Task<DomainEntities.MdcActionItem?> FindActionItemAsync(Guid id, CancellationToken ct) =>
        db.MdcActionItems.FindAsync([id], ct).AsTask();

    public async Task AddActionItemAsync(DomainEntities.MdcActionItem item, CancellationToken ct) =>
        await db.MdcActionItems.AddAsync(item, ct);

    public async Task<IReadOnlyList<DecisionWithTalentsData>> GetDecisionsByTalentAsync(Guid talentId, CancellationToken ct)
    {
        var decisionIds = await db.MdcDecisionTalents
            .Where(t => t.TalentId == talentId)
            .Select(t => t.DecisionId)
            .ToListAsync(ct);

        var decisions = await db.MdcDecisions
            .Include(d => d.Talents)
            .Where(d => decisionIds.Contains(d.Id))
            .OrderByDescending(d => d.DecidedAt)
            .ToListAsync(ct);

        var allTalentIds = decisions.SelectMany(d => d.Talents.Select(t => t.TalentId)).Distinct().ToList();
        var users = await db.Users.Where(u => allTalentIds.Contains(u.Id)).ToListAsync(ct);
        var userMap = users.ToDictionary(u => u.Id, u => u.DisplayName);

        return decisions.Select(d => new DecisionWithTalentsData(
            d,
            d.Talents.Select(t => (t.TalentId, userMap.GetValueOrDefault(t.TalentId, string.Empty))).ToList()
        )).ToList();
    }

    public async Task<IReadOnlyList<ActionItemWithSessionData>> GetActionItemsByAssigneeAsync(Guid assigneeId, bool pendingOnly, CancellationToken ct)
    {
        var query = db.MdcActionItems.Where(a => a.AssigneeId == assigneeId);
        if (pendingOnly)
            query = query.Where(a => a.Status == MdcActionItemStatus.NotStarted || a.Status == MdcActionItemStatus.InProgress);

        var items = await query.OrderBy(a => a.DueDate).ToListAsync(ct);
        var assignee = await db.Users.FindAsync([assigneeId], ct);
        var name = assignee?.DisplayName ?? string.Empty;

        return items.Select(a => new ActionItemWithSessionData(a, name)).ToList();
    }

    public async Task<IReadOnlyList<DomainEntities.User>> GetUsersByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var idList = ids.ToList();
        if (idList.Count == 0) return [];
        return await db.Users.Where(u => idList.Contains(u.Id)).ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
