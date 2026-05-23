using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.CommitteeSession;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class CommitteeSessionRepository(AppDbContext db) : ICommitteeSessionRepository
{
    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FindAsync([userId], ct).AsTask();

    public async Task<IReadOnlyList<CommitteeSession>> GetAllAsync(CancellationToken ct) =>
        await db.CommitteeSessions
            .Include(s => s.Participants)
            .Include(s => s.KeyPersons)
            .OrderByDescending(s => s.HeldOn)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CommitteeSession>> GetListForManagerAsync(Guid managerId, CancellationToken ct) =>
        await db.CommitteeSessions
            .Include(s => s.Participants)
            .Include(s => s.KeyPersons)
            .Where(s => s.KeyPersons.Any(kp =>
                db.KeyPersons
                    .Where(k => k.Id == kp.KeyPersonId)
                    .Any(k => db.Users.Any(u => u.Id == k.TalentId && u.ManagerId == managerId))))
            .OrderByDescending(s => s.HeldOn)
            .ToListAsync(ct);

    public async Task<CommitteeSession?> FindAsync(Guid id, CancellationToken ct) =>
        await db.CommitteeSessions
            .Include(s => s.Participants)
            .Include(s => s.KeyPersons)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(CommitteeSession session, CancellationToken ct) =>
        await db.CommitteeSessions.AddAsync(session, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<CommitteeSummaryData>> GetCommitteeSummaryDataAsync(CancellationToken ct)
    {
        var currentYear = DateTime.UtcNow.Year;

        var keyPersons = await db.KeyPersons
            .Where(k => k.IsActive)
            .OrderBy(k => k.DesignatedAt)
            .ToListAsync(ct);

        var talentIds = keyPersons.Select(k => k.TalentId).ToList();
        var keyPersonIds = keyPersons.Select(k => k.Id).ToList();

        var talents = await db.Users
            .Where(u => talentIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        var plans = await db.CareerPlans
            .Where(p => talentIds.Contains(p.UserId) && p.PeriodYear == currentYear)
            .ToDictionaryAsync(p => p.UserId, ct);

        var skillCounts = await db.UserSkills
            .Where(s => talentIds.Contains(s.UserId) && s.Status == UserSkillStatus.Approved)
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, ct);

        var keyPersonsWithPlans = await db.DevelopmentPlans
            .Where(d => keyPersonIds.Contains(d.KeyPersonId))
            .Select(d => d.KeyPersonId)
            .Distinct()
            .ToListAsync(ct);

        var keyPersonsWithPlansSet = new HashSet<Guid>(keyPersonsWithPlans);

        return keyPersons.Select(k =>
        {
            var talent = talents.GetValueOrDefault(k.TalentId);
            var plan = plans.GetValueOrDefault(k.TalentId);
            return new CommitteeSummaryData(
                k.Id,
                k.TalentId,
                talent?.DisplayName ?? string.Empty,
                plan?.LongTermGoal,
                plan?.CurrentFocus,
                skillCounts.GetValueOrDefault(k.TalentId),
                keyPersonsWithPlansSet.Contains(k.Id)
            );
        }).ToList();
    }
}
