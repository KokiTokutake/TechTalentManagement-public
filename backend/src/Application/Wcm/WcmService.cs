using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Enums;
using DomainEntities = TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Application.Wcm;

// ── DTOs ────────────────────────────────────────────────────────────────────

public record WcmEntryDto(Guid Id, string Content, int DisplayOrder, Guid? LinkedSkillItemId, string? LinkedSkillName);

public record WcmWillDto(
    IReadOnlyList<WcmEntryDto> ShortTerm,
    IReadOnlyList<WcmEntryDto> MidTerm,
    IReadOnlyList<WcmEntryDto> LongTerm);

public record WcmCanDto(
    IReadOnlyList<WcmEntryDto> Strengths,
    IReadOnlyList<WcmEntryDto> Challenges);

public record WcmDto(
    Guid TalentId,
    Guid PeriodId,
    WcmWillDto Will,
    WcmCanDto Can,
    IReadOnlyList<WcmEntryDto> Must,
    string? ManagerExpectedRole);

// ── Save Request DTOs ────────────────────────────────────────────────────────

public record SaveWcmEntryRequest(Guid? Id, string Content, Guid? LinkedSkillItemId = null);

public record SaveWcmWillRequest(
    IReadOnlyList<SaveWcmEntryRequest>? ShortTerm,
    IReadOnlyList<SaveWcmEntryRequest>? MidTerm,
    IReadOnlyList<SaveWcmEntryRequest>? LongTerm);

public record SaveWcmCanRequest(
    IReadOnlyList<SaveWcmEntryRequest>? Strengths,
    IReadOnlyList<SaveWcmEntryRequest>? Challenges);

public record SaveWcmRequest(
    Guid TalentId,
    Guid PeriodId,
    SaveWcmWillRequest? Will,
    SaveWcmCanRequest? Can,
    IReadOnlyList<SaveWcmEntryRequest>? Must);

// ── Repository Interface ─────────────────────────────────────────────────────

public interface IWcmRepository
{
    Task<DomainEntities.User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<DomainEntities.EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct);
    Task<IReadOnlyList<DomainEntities.WillCanMustEntry>> GetEntriesAsync(Guid talentId, Guid periodId, CancellationToken ct);
    Task<IReadOnlyList<DomainEntities.SkillItem>> GetSkillItemsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<DomainEntities.TalentManagerComment?> FindManagerCommentAsync(Guid talentId, Guid periodId, CancellationToken ct);
    Task RemoveEntriesAsync(Guid talentId, Guid periodId, CancellationToken ct);
    Task AddEntriesAsync(IEnumerable<DomainEntities.WillCanMustEntry> entries, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

// ── Service Interface ────────────────────────────────────────────────────────

public interface IWcmService
{
    Task<Result<WcmDto>> GetWcmAsync(Guid requesterId, Guid talentId, Guid periodId, CancellationToken ct = default);
    Task<Result<WcmDto>> SaveWcmAsync(Guid requesterId, SaveWcmRequest req, CancellationToken ct = default);
}

// ── Service Implementation ───────────────────────────────────────────────────

public class WcmService(IWcmRepository repo) : IWcmService
{
    private static readonly HashSet<UserRole> SeniorRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];
    private const int CanItemLimit = 5;

    public async Task<Result<WcmDto>> GetWcmAsync(Guid requesterId, Guid talentId, Guid periodId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<WcmDto>.Forbidden("権限がありません。");

        if (!CanView(requester, talentId))
            return Result<WcmDto>.Forbidden("このタレントのWCMを閲覧する権限がありません。");

        var period = await repo.FindPeriodAsync(periodId, ct);
        if (period is null)
            return Result<WcmDto>.NotFound("考課期間が見つかりません。");

        var talent = await repo.FindUserAsync(talentId, ct);
        if (talent is null)
            return Result<WcmDto>.NotFound("タレントが見つかりません。");

        var entries = await repo.GetEntriesAsync(talentId, periodId, ct);
        var skillIds = entries.Where(e => e.LinkedSkillItemId.HasValue).Select(e => e.LinkedSkillItemId!.Value).Distinct();
        var skills = await repo.GetSkillItemsByIdsAsync(skillIds, ct);
        var skillMap = skills.ToDictionary(s => s.Id, s => s.Name);

        var comment = await repo.FindManagerCommentAsync(talentId, periodId, ct);
        var dto = BuildDto(talentId, periodId, entries, skillMap, comment?.ExpectedRole);
        return Result<WcmDto>.Ok(dto);
    }

    public async Task<Result<WcmDto>> SaveWcmAsync(Guid requesterId, SaveWcmRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<WcmDto>.Forbidden("権限がありません。");

        // 保存は本人のみ
        if (requesterId != req.TalentId)
            return Result<WcmDto>.Forbidden("自分のWCMのみ保存できます。");

        var period = await repo.FindPeriodAsync(req.PeriodId, ct);
        if (period is null)
            return Result<WcmDto>.NotFound("考課期間が見つかりません。");

        var talent = await repo.FindUserAsync(req.TalentId, ct);
        if (talent is null)
            return Result<WcmDto>.NotFound("タレントが見つかりません。");

        // Can 上限チェック
        var strengthCount = req.Can?.Strengths?.Count ?? 0;
        var challengeCount = req.Can?.Challenges?.Count ?? 0;
        if (strengthCount > CanItemLimit)
            return Result<WcmDto>.BadRequest($"強みは最大{CanItemLimit}件まで入力できます。");
        if (challengeCount > CanItemLimit)
            return Result<WcmDto>.BadRequest($"課題は最大{CanItemLimit}件まで入力できます。");

        // content バリデーション
        var allEntries = CollectAllEntries(req);
        if (allEntries.Any(e => string.IsNullOrWhiteSpace(e.Content) || e.Content.Length > 2000))
            return Result<WcmDto>.BadRequest("各項目は1〜2000文字で入力してください。");

        // 全置換
        await repo.RemoveEntriesAsync(req.TalentId, req.PeriodId, ct);

        var newEntries = BuildEntries(req);
        await repo.AddEntriesAsync(newEntries, ct);
        await repo.SaveChangesAsync(ct);

        var saved = await repo.GetEntriesAsync(req.TalentId, req.PeriodId, ct);
        var skillIds = saved.Where(e => e.LinkedSkillItemId.HasValue).Select(e => e.LinkedSkillItemId!.Value).Distinct();
        var skills = await repo.GetSkillItemsByIdsAsync(skillIds, ct);
        var skillMap = skills.ToDictionary(s => s.Id, s => s.Name);

        var comment = await repo.FindManagerCommentAsync(req.TalentId, req.PeriodId, ct);
        return Result<WcmDto>.Ok(BuildDto(req.TalentId, req.PeriodId, saved, skillMap, comment?.ExpectedRole));
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static bool CanView(DomainEntities.User requester, Guid talentId)
    {
        if (requester.Id == talentId) return true;
        if (SeniorRoles.Contains(requester.Role)) return true;
        if (requester.Role == UserRole.Manager) return true; // 部下チェックはシンプル化
        return false;
    }

    private static IEnumerable<SaveWcmEntryRequest> CollectAllEntries(SaveWcmRequest req)
    {
        foreach (var e in req.Will?.ShortTerm ?? []) yield return e;
        foreach (var e in req.Will?.MidTerm ?? []) yield return e;
        foreach (var e in req.Will?.LongTerm ?? []) yield return e;
        foreach (var e in req.Can?.Strengths ?? []) yield return e;
        foreach (var e in req.Can?.Challenges ?? []) yield return e;
        foreach (var e in req.Must ?? []) yield return e;
    }

    private static List<DomainEntities.WillCanMustEntry> BuildEntries(SaveWcmRequest req)
    {
        var entries = new List<DomainEntities.WillCanMustEntry>();

        AddWillEntries(entries, req.Will?.ShortTerm, WcmTimeHorizon.ShortTerm, req.TalentId, req.PeriodId);
        AddWillEntries(entries, req.Will?.MidTerm, WcmTimeHorizon.MidTerm, req.TalentId, req.PeriodId);
        AddWillEntries(entries, req.Will?.LongTerm, WcmTimeHorizon.LongTerm, req.TalentId, req.PeriodId);
        AddCanEntries(entries, req.Can?.Strengths, CanType.Strength, req.TalentId, req.PeriodId);
        AddCanEntries(entries, req.Can?.Challenges, CanType.Challenge, req.TalentId, req.PeriodId);
        AddMustEntries(entries, req.Must, req.TalentId, req.PeriodId);

        return entries;
    }

    private static void AddWillEntries(List<DomainEntities.WillCanMustEntry> entries,
        IReadOnlyList<SaveWcmEntryRequest>? list, WcmTimeHorizon horizon, Guid talentId, Guid periodId)
    {
        if (list is null) return;
        for (int i = 0; i < list.Count; i++)
            entries.Add(DomainEntities.WillCanMustEntry.Create(talentId, periodId, WcmCategory.Will, list[i].Content, i, timeHorizon: horizon));
    }

    private static void AddCanEntries(List<DomainEntities.WillCanMustEntry> entries,
        IReadOnlyList<SaveWcmEntryRequest>? list, CanType canType, Guid talentId, Guid periodId)
    {
        if (list is null) return;
        for (int i = 0; i < list.Count; i++)
            entries.Add(DomainEntities.WillCanMustEntry.Create(talentId, periodId, WcmCategory.Can, list[i].Content, i, canType: canType, linkedSkillItemId: list[i].LinkedSkillItemId));
    }

    private static void AddMustEntries(List<DomainEntities.WillCanMustEntry> entries,
        IReadOnlyList<SaveWcmEntryRequest>? list, Guid talentId, Guid periodId)
    {
        if (list is null) return;
        for (int i = 0; i < list.Count; i++)
            entries.Add(DomainEntities.WillCanMustEntry.Create(talentId, periodId, WcmCategory.Must, list[i].Content, i));
    }

    private static WcmDto BuildDto(
        Guid talentId, Guid periodId,
        IReadOnlyList<DomainEntities.WillCanMustEntry> entries,
        Dictionary<Guid, string> skillMap,
        string? managerExpectedRole)
    {
        WcmEntryDto ToEntry(DomainEntities.WillCanMustEntry e) =>
            new(e.Id, e.Content, e.DisplayOrder, e.LinkedSkillItemId,
                e.LinkedSkillItemId.HasValue ? skillMap.GetValueOrDefault(e.LinkedSkillItemId.Value) : null);

        var willEntries = entries.Where(e => e.Category == WcmCategory.Will).OrderBy(e => e.DisplayOrder);
        var canEntries = entries.Where(e => e.Category == WcmCategory.Can).OrderBy(e => e.DisplayOrder);
        var mustEntries = entries.Where(e => e.Category == WcmCategory.Must).OrderBy(e => e.DisplayOrder);

        var will = new WcmWillDto(
            willEntries.Where(e => e.TimeHorizon == WcmTimeHorizon.ShortTerm).Select(ToEntry).ToList(),
            willEntries.Where(e => e.TimeHorizon == WcmTimeHorizon.MidTerm).Select(ToEntry).ToList(),
            willEntries.Where(e => e.TimeHorizon == WcmTimeHorizon.LongTerm).Select(ToEntry).ToList());

        var can = new WcmCanDto(
            canEntries.Where(e => e.CanType == CanType.Strength).Select(ToEntry).ToList(),
            canEntries.Where(e => e.CanType == CanType.Challenge).Select(ToEntry).ToList());

        var must = mustEntries.Select(ToEntry).ToList();

        return new WcmDto(talentId, periodId, will, can, must, managerExpectedRole);
    }
}
