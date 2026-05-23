using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.Wcm;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class WcmRepository(AppDbContext db) : IWcmRepository
{
    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task<EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct) =>
        db.EvaluationPeriods.FirstOrDefaultAsync(p => p.Id == periodId, ct);

    public async Task<IReadOnlyList<WillCanMustEntry>> GetEntriesAsync(Guid talentId, Guid periodId, CancellationToken ct) =>
        await db.WillCanMustEntries
            .Where(e => e.TalentId == talentId && e.PeriodId == periodId)
            .OrderBy(e => e.Category).ThenBy(e => e.DisplayOrder)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SkillItem>> GetSkillItemsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct) =>
        await db.SkillItems.Where(s => ids.Contains(s.Id)).ToListAsync(ct);

    public Task<TalentManagerComment?> FindManagerCommentAsync(Guid talentId, Guid periodId, CancellationToken ct) =>
        db.TalentManagerComments.FirstOrDefaultAsync(c => c.TalentId == talentId && c.PeriodId == periodId, ct);

    public async Task RemoveEntriesAsync(Guid talentId, Guid periodId, CancellationToken ct)
    {
        var existing = await db.WillCanMustEntries
            .Where(e => e.TalentId == talentId && e.PeriodId == periodId)
            .ToListAsync(ct);
        db.WillCanMustEntries.RemoveRange(existing);
    }

    public async Task AddEntriesAsync(IEnumerable<WillCanMustEntry> entries, CancellationToken ct) =>
        await db.WillCanMustEntries.AddRangeAsync(entries, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
