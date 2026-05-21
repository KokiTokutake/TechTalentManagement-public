using TeckTalentManagement.Application.Common;
using TeckTalentManagement.Domain.Entities;
using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Application.OneOnOne;

public interface IOneOnOneService
{
    Task<Result<MeetingListResponse>> GetMeetingsAsync(Guid talentId, int page, int limit, CancellationToken ct = default);
    Task<Result<MeetingDetailDto>> CreateMeetingAsync(Guid managerId, CreateMeetingRequest req, CancellationToken ct = default);
    Task<Result<MeetingDetailDto>> GetMeetingAsync(Guid meetingId, Guid requesterId, CancellationToken ct = default);
    Task<Result<MeetingDetailDto>> UpdateMeetingAsync(Guid meetingId, Guid managerId, UpdateMeetingRequest req, CancellationToken ct = default);
    Task<Result<bool>> DeleteMeetingAsync(Guid meetingId, Guid managerId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ActionItemDto>>> GetActionItemsAsync(Guid meetingId, CancellationToken ct = default);
    Task<Result<ActionItemDto>> AddActionItemAsync(Guid meetingId, Guid managerId, AddActionItemRequest req, CancellationToken ct = default);
    Task<Result<ActionItemDto>> UpdateActionItemStatusAsync(Guid itemId, Guid managerId, UpdateActionItemStatusRequest req, CancellationToken ct = default);
    Task<Result<OneOnOneSummaryDto>> GetOneOnOneSummaryAsync(Guid talentId, CancellationToken ct = default);
    Task<Result<MeetingDetailDto?>> GetPreviousMeetingAsync(Guid talentId, CancellationToken ct = default);
}

public interface IOneOnOneRepository
{
    Task<(IReadOnlyList<OneOnOneMeeting> Items, int TotalCount)> GetPagedByTalentAsync(Guid talentId, int page, int limit, CancellationToken ct);
    Task<OneOnOneMeeting?> FindAsync(Guid meetingId, CancellationToken ct);
    Task<OneOnOneMeeting?> FindWithItemsAsync(Guid meetingId, CancellationToken ct);
    Task<User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task AddAsync(OneOnOneMeeting meeting, CancellationToken ct);
    Task<MeetingActionItem?> FindActionItemAsync(Guid itemId, CancellationToken ct);
    Task AddActionItemAsync(MeetingActionItem item, CancellationToken ct);
    Task<IReadOnlyList<OneOnOneMeeting>> GetSharedMeetingsByTalentAsync(Guid talentId, CancellationToken ct);
    Task<int> CountByTalentAsync(Guid talentId, CancellationToken ct);
    Task<DateOnly?> GetLastMeetingDateAsync(Guid talentId, CancellationToken ct);
    Task<OneOnOneMeeting?> GetLatestMeetingWithItemsAsync(Guid talentId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class OneOnOneService(IOneOnOneRepository repo) : IOneOnOneService
{
    public async Task<Result<MeetingListResponse>> GetMeetingsAsync(Guid talentId, int page, int limit, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 100);
        page = Math.Max(1, page);

        var talent = await repo.FindUserAsync(talentId, ct);
        if (talent is null)
            return Result<MeetingListResponse>.NotFound("タレントが見つかりません。");

        var (items, total) = await repo.GetPagedByTalentAsync(talentId, page, limit, ct);
        var dtos = items.Select(ToListItemDto).ToList();

        return Result<MeetingListResponse>.Ok(new MeetingListResponse(dtos, total, page, limit));
    }

    public async Task<Result<MeetingDetailDto>> CreateMeetingAsync(Guid managerId, CreateMeetingRequest req, CancellationToken ct = default)
    {
        if (req.ConductedAt == default)
            return Result<MeetingDetailDto>.BadRequest("実施日は必須です。");

        if (!TryParseFormat(req.Format, out var format))
            return Result<MeetingDetailDto>.BadRequest($"形式が不正です: {req.Format}");

        var talent = await repo.FindUserAsync(req.TalentId, ct);
        if (talent is null)
            return Result<MeetingDetailDto>.NotFound("タレントが見つかりません。");

        var manager = await repo.FindUserAsync(managerId, ct);
        if (manager is null)
            return Result<MeetingDetailDto>.NotFound("上長が見つかりません。");

        var meeting = OneOnOneMeeting.Create(
            req.TalentId, managerId, req.ConductedAt, format,
            req.WillTopics, req.CanTopics, req.MustTopics,
            req.CurrentSituation, req.FreeText, req.ShareWithMdc);

        await repo.AddAsync(meeting, ct);

        if (req.ActionItems is { Count: > 0 })
        {
            foreach (var desc in req.ActionItems)
            {
                var item = MeetingActionItem.Create(meeting.Id, desc);
                await repo.AddActionItemAsync(item, ct);
            }
        }

        await repo.SaveChangesAsync(ct);

        var saved = await repo.FindWithItemsAsync(meeting.Id, ct);
        return Result<MeetingDetailDto>.Ok(ToDetailDto(saved!, talent, manager));
    }

    public async Task<Result<MeetingDetailDto>> GetMeetingAsync(Guid meetingId, Guid requesterId, CancellationToken ct = default)
    {
        var meeting = await repo.FindWithItemsAsync(meetingId, ct);
        if (meeting is null)
            return Result<MeetingDetailDto>.NotFound("面談記録が見つかりません。");

        var talent = await repo.FindUserAsync(meeting.TalentId, ct);
        var manager = await repo.FindUserAsync(meeting.ManagerId, ct);

        return Result<MeetingDetailDto>.Ok(ToDetailDto(meeting, talent!, manager!));
    }

    public async Task<Result<MeetingDetailDto>> UpdateMeetingAsync(Guid meetingId, Guid managerId, UpdateMeetingRequest req, CancellationToken ct = default)
    {
        if (!TryParseFormat(req.Format, out var format))
            return Result<MeetingDetailDto>.BadRequest($"形式が不正です: {req.Format}");

        var meeting = await repo.FindWithItemsAsync(meetingId, ct);
        if (meeting is null)
            return Result<MeetingDetailDto>.NotFound("面談記録が見つかりません。");

        if (meeting.ManagerId != managerId)
            return Result<MeetingDetailDto>.Forbidden("他の上長が作成した記録は更新できません。");

        meeting.Update(req.ConductedAt, format, req.WillTopics, req.CanTopics,
            req.MustTopics, req.CurrentSituation, req.FreeText, req.ShareWithMdc);

        if (req.ActionItems is { Count: > 0 })
        {
            foreach (var desc in req.ActionItems)
            {
                var item = MeetingActionItem.Create(meeting.Id, desc);
                await repo.AddActionItemAsync(item, ct);
            }
        }

        await repo.SaveChangesAsync(ct);

        var updated = await repo.FindWithItemsAsync(meeting.Id, ct);
        var talent = await repo.FindUserAsync(meeting.TalentId, ct);
        var manager = await repo.FindUserAsync(meeting.ManagerId, ct);

        return Result<MeetingDetailDto>.Ok(ToDetailDto(updated!, talent!, manager!));
    }

    public async Task<Result<bool>> DeleteMeetingAsync(Guid meetingId, Guid managerId, CancellationToken ct = default)
    {
        var meeting = await repo.FindAsync(meetingId, ct);
        if (meeting is null)
            return Result<bool>.NotFound("面談記録が見つかりません。");

        if (meeting.ManagerId != managerId)
            return Result<bool>.Forbidden("他の上長が作成した記録は削除できません。");

        meeting.Delete();
        await repo.SaveChangesAsync(ct);

        return Result<bool>.Ok(true);
    }

    public async Task<Result<IReadOnlyList<ActionItemDto>>> GetActionItemsAsync(Guid meetingId, CancellationToken ct = default)
    {
        var meeting = await repo.FindWithItemsAsync(meetingId, ct);
        if (meeting is null)
            return Result<IReadOnlyList<ActionItemDto>>.NotFound("面談記録が見つかりません。");

        var dtos = meeting.ActionItems.Select(ToActionItemDto).ToList();
        return Result<IReadOnlyList<ActionItemDto>>.Ok(dtos);
    }

    public async Task<Result<ActionItemDto>> AddActionItemAsync(Guid meetingId, Guid managerId, AddActionItemRequest req, CancellationToken ct = default)
    {
        var meeting = await repo.FindAsync(meetingId, ct);
        if (meeting is null)
            return Result<ActionItemDto>.NotFound("面談記録が見つかりません。");

        if (meeting.ManagerId != managerId)
            return Result<ActionItemDto>.Forbidden("他の上長が作成した記録に宿題を追加できません。");

        var item = MeetingActionItem.Create(meetingId, req.Description);
        await repo.AddActionItemAsync(item, ct);
        await repo.SaveChangesAsync(ct);

        return Result<ActionItemDto>.Ok(ToActionItemDto(item));
    }

    public async Task<Result<ActionItemDto>> UpdateActionItemStatusAsync(Guid itemId, Guid managerId, UpdateActionItemStatusRequest req, CancellationToken ct = default)
    {
        var item = await repo.FindActionItemAsync(itemId, ct);
        if (item is null)
            return Result<ActionItemDto>.NotFound("宿題が見つかりません。");

        var meeting = await repo.FindAsync(item.MeetingId, ct);
        if (meeting is null || meeting.ManagerId != managerId)
            return Result<ActionItemDto>.Forbidden("この宿題を更新する権限がありません。");

        if (req.Status == "Done")
            item.Complete();
        else if (req.Status == "Pending")
            item.Reopen();
        else
            return Result<ActionItemDto>.BadRequest($"ステータスが不正です: {req.Status}");

        await repo.SaveChangesAsync(ct);

        return Result<ActionItemDto>.Ok(ToActionItemDto(item));
    }

    public async Task<Result<OneOnOneSummaryDto>> GetOneOnOneSummaryAsync(Guid talentId, CancellationToken ct = default)
    {
        var talent = await repo.FindUserAsync(talentId, ct);
        if (talent is null)
            return Result<OneOnOneSummaryDto>.NotFound("タレントが見つかりません。");

        var total = await repo.CountByTalentAsync(talentId, ct);
        var lastDate = await repo.GetLastMeetingDateAsync(talentId, ct);
        var shared = await repo.GetSharedMeetingsByTalentAsync(talentId, ct);

        var sharedDtos = shared.Select(m => new SharedMeetingSummaryDto(
            m.Id,
            m.ConductedAt,
            BuildNotesSummary(m)
        )).ToList();

        var talentName = talent.DisplayName;

        return Result<OneOnOneSummaryDto>.Ok(new OneOnOneSummaryDto(
            talentId, talentName, lastDate, total, sharedDtos));
    }

    public async Task<Result<MeetingDetailDto?>> GetPreviousMeetingAsync(Guid talentId, CancellationToken ct = default)
    {
        var talent = await repo.FindUserAsync(talentId, ct);
        if (talent is null)
            return Result<MeetingDetailDto?>.NotFound("タレントが見つかりません。");

        var meeting = await repo.GetLatestMeetingWithItemsAsync(talentId, ct);
        if (meeting is null)
            return Result<MeetingDetailDto?>.Ok(null);

        var manager = await repo.FindUserAsync(meeting.ManagerId, ct);
        return Result<MeetingDetailDto?>.Ok(ToDetailDto(meeting, talent, manager!));
    }

    private static bool TryParseFormat(string raw, out MeetingFormat format)
        => Enum.TryParse(raw, ignoreCase: true, out format);

    private static string? BuildNotesSummary(OneOnOneMeeting m)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(m.WillTopics)) parts.Add($"Will: {m.WillTopics[..Math.Min(m.WillTopics.Length, 50)]}");
        if (!string.IsNullOrEmpty(m.CanTopics)) parts.Add($"Can: {m.CanTopics[..Math.Min(m.CanTopics.Length, 50)]}");
        if (!string.IsNullOrEmpty(m.MustTopics)) parts.Add($"Must: {m.MustTopics[..Math.Min(m.MustTopics.Length, 50)]}");
        return parts.Count > 0 ? string.Join(" / ", parts) : null;
    }

    private static MeetingListItemDto ToListItemDto(OneOnOneMeeting m) => new(
        m.Id,
        m.TalentId,
        m.Talent?.DisplayName ?? string.Empty,
        m.ManagerId,
        m.Manager?.DisplayName ?? string.Empty,
        m.ConductedAt,
        m.Format.ToString(),
        BuildNotesSummary(m),
        m.ActionItems.Count,
        m.ActionItems.Count(a => a.Status == ActionItemStatus.Pending),
        m.ShareWithMdc,
        m.CreatedAt
    );

    private static MeetingDetailDto ToDetailDto(OneOnOneMeeting m, User talent, User manager) => new(
        m.Id,
        m.TalentId,
        talent.DisplayName,
        m.ManagerId,
        manager.DisplayName,
        m.ConductedAt,
        m.Format.ToString(),
        m.WillTopics,
        m.CanTopics,
        m.MustTopics,
        m.CurrentSituation,
        m.FreeText,
        m.ShareWithMdc,
        m.ActionItems.Select(ToActionItemDto).ToList(),
        m.CreatedAt,
        m.UpdatedAt
    );

    private static ActionItemDto ToActionItemDto(MeetingActionItem a) => new(
        a.Id,
        a.Description,
        a.Status.ToString(),
        a.CompletedAt,
        a.CreatedAt
    );
}
