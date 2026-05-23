using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.OneOnOne;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class OneOnOneRepository(AppDbContext db) : IOneOnOneRepository
{
    public async Task<(IReadOnlyList<OneOnOneMeeting> Items, int TotalCount)> GetPagedByTalentAsync(
        Guid talentId, int page, int limit, CancellationToken ct)
    {
        var query = db.OneOnOneMeetings
            .Include(m => m.Talent)
            .Include(m => m.Manager)
            .Include(m => m.ActionItems)
            .Where(m => m.TalentId == talentId)
            .OrderByDescending(m => m.ConductedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<OneOnOneMeeting?> FindAsync(Guid meetingId, CancellationToken ct) =>
        db.OneOnOneMeetings.FirstOrDefaultAsync(m => m.Id == meetingId, ct);

    public Task<OneOnOneMeeting?> FindWithItemsAsync(Guid meetingId, CancellationToken ct) =>
        db.OneOnOneMeetings
            .Include(m => m.ActionItems)
            .FirstOrDefaultAsync(m => m.Id == meetingId, ct);

    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public async Task AddAsync(OneOnOneMeeting meeting, CancellationToken ct) =>
        await db.OneOnOneMeetings.AddAsync(meeting, ct);

    public async Task AddActionItemAsync(MeetingActionItem item, CancellationToken ct) =>
        await db.MeetingActionItems.AddAsync(item, ct);

    public Task<MeetingActionItem?> FindActionItemAsync(Guid itemId, CancellationToken ct) =>
        db.MeetingActionItems.FirstOrDefaultAsync(a => a.Id == itemId, ct);

    public async Task<IReadOnlyList<OneOnOneMeeting>> GetSharedMeetingsByTalentAsync(Guid talentId, CancellationToken ct) =>
        await db.OneOnOneMeetings
            .Where(m => m.TalentId == talentId && m.ShareWithMdc)
            .OrderByDescending(m => m.ConductedAt)
            .ToListAsync(ct);

    public Task<int> CountByTalentAsync(Guid talentId, CancellationToken ct) =>
        db.OneOnOneMeetings.CountAsync(m => m.TalentId == talentId, ct);

    public async Task<DateOnly?> GetLastMeetingDateAsync(Guid talentId, CancellationToken ct)
    {
        var last = await db.OneOnOneMeetings
            .Where(m => m.TalentId == talentId)
            .OrderByDescending(m => m.ConductedAt)
            .Select(m => (DateOnly?)m.ConductedAt)
            .FirstOrDefaultAsync(ct);
        return last;
    }

    public Task<OneOnOneMeeting?> GetLatestMeetingWithItemsAsync(Guid talentId, CancellationToken ct) =>
        db.OneOnOneMeetings
            .Include(m => m.ActionItems)
            .Where(m => m.TalentId == talentId)
            .OrderByDescending(m => m.ConductedAt)
            .FirstOrDefaultAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
