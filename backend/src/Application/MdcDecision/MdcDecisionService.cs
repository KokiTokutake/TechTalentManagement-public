using TeckTalentManagement.Application.Common;
using TeckTalentManagement.Domain.Enums;
using DomainEntities = TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Application.MdcDecision;

// ── DTOs ────────────────────────────────────────────────────────────────────

public record MdcDecisionDto(
    Guid Id,
    Guid SessionId,
    string DecisionType,
    string Content,
    DateOnly DecidedAt,
    Guid CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<DecisionTalentDto> Talents,
    IReadOnlyList<MdcActionItemDto> ActionItems
);

public record DecisionTalentDto(Guid TalentId, string TalentName);

public record MdcActionItemDto(
    Guid Id,
    Guid SessionId,
    Guid? DecisionId,
    string Description,
    Guid AssigneeId,
    string AssigneeName,
    DateOnly? DueDate,
    string Status,
    DateTimeOffset? CompletedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record CreateDecisionRequest(
    string DecisionType,
    string Content,
    DateOnly DecidedAt,
    IReadOnlyList<Guid> TalentIds
);

public record UpdateDecisionRequest(
    string DecisionType,
    string Content,
    DateOnly DecidedAt,
    IReadOnlyList<Guid> TalentIds
);

public record CreateActionItemRequest(
    string Description,
    Guid AssigneeId,
    DateOnly? DueDate
);

public record UpdateActionItemStatusRequest(string Status);

// ── Repository Interface ─────────────────────────────────────────────────────

public interface IMdcDecisionRepository
{
    Task<DomainEntities.User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<DomainEntities.CommitteeSession?> FindSessionAsync(Guid sessionId, CancellationToken ct);
    Task<IReadOnlyList<DomainEntities.MdcDecision>> GetDecisionsBySessionAsync(Guid sessionId, CancellationToken ct);
    Task<DomainEntities.MdcDecision?> FindDecisionAsync(Guid id, CancellationToken ct);
    Task AddDecisionAsync(DomainEntities.MdcDecision decision, CancellationToken ct);
    Task<IReadOnlyList<DomainEntities.MdcActionItem>> GetActionItemsBySessionAsync(Guid sessionId, MdcActionItemStatus? status, CancellationToken ct);
    Task<DomainEntities.MdcActionItem?> FindActionItemAsync(Guid id, CancellationToken ct);
    Task AddActionItemAsync(DomainEntities.MdcActionItem item, CancellationToken ct);
    Task<IReadOnlyList<DecisionWithTalentsData>> GetDecisionsByTalentAsync(Guid talentId, CancellationToken ct);
    Task<IReadOnlyList<ActionItemWithSessionData>> GetActionItemsByAssigneeAsync(Guid assigneeId, bool pendingOnly, CancellationToken ct);
    Task<IReadOnlyList<DomainEntities.User>> GetUsersByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public record DecisionWithTalentsData(
    DomainEntities.MdcDecision Decision,
    IReadOnlyList<(Guid TalentId, string TalentName)> Talents
);

public record ActionItemWithSessionData(
    DomainEntities.MdcActionItem ActionItem,
    string AssigneeName
);

// ── Service Interface ────────────────────────────────────────────────────────

public interface IMdcDecisionService
{
    Task<Result<IReadOnlyList<MdcDecisionDto>>> GetDecisionsAsync(Guid requesterId, Guid sessionId, CancellationToken ct = default);
    Task<Result<MdcDecisionDto>> CreateDecisionAsync(Guid requesterId, Guid sessionId, CreateDecisionRequest req, CancellationToken ct = default);
    Task<Result<MdcDecisionDto>> UpdateDecisionAsync(Guid requesterId, Guid decisionId, UpdateDecisionRequest req, CancellationToken ct = default);
    Task<Result<MdcActionItemDto>> AddActionItemAsync(Guid requesterId, Guid decisionId, CreateActionItemRequest req, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MdcActionItemDto>>> GetActionItemsAsync(Guid requesterId, Guid sessionId, MdcActionItemStatus? status, CancellationToken ct = default);
    Task<Result<MdcActionItemDto>> UpdateActionItemStatusAsync(Guid requesterId, Guid actionItemId, UpdateActionItemStatusRequest req, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MdcDecisionDto>>> GetDecisionsByTalentAsync(Guid requesterId, Guid talentId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MdcActionItemDto>>> GetActionItemsByUserAsync(Guid requesterId, Guid userId, bool pendingOnly, CancellationToken ct = default);
}

// ── Service Implementation ───────────────────────────────────────────────────

public class MdcDecisionService(IMdcDecisionRepository repo) : IMdcDecisionService
{
    private static readonly HashSet<UserRole> SeniorRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<IReadOnlyList<MdcDecisionDto>>> GetDecisionsAsync(Guid requesterId, Guid sessionId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !SeniorRoles.Contains(requester.Role))
            return Result<IReadOnlyList<MdcDecisionDto>>.Forbidden("決定事項を閲覧する権限がありません。");

        var session = await repo.FindSessionAsync(sessionId, ct);
        if (session is null)
            return Result<IReadOnlyList<MdcDecisionDto>>.NotFound("セッションが見つかりません。");

        var decisions = await repo.GetDecisionsBySessionAsync(sessionId, ct);
        var dtos = await ToDecisionDtosAsync(decisions, ct);
        return Result<IReadOnlyList<MdcDecisionDto>>.Ok(dtos);
    }

    public async Task<Result<MdcDecisionDto>> CreateDecisionAsync(Guid requesterId, Guid sessionId, CreateDecisionRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !SeniorRoles.Contains(requester.Role))
            return Result<MdcDecisionDto>.Forbidden("決定事項を作成する権限がありません。");

        var session = await repo.FindSessionAsync(sessionId, ct);
        if (session is null)
            return Result<MdcDecisionDto>.NotFound("セッションが見つかりません。");

        if (!Enum.TryParse<DecisionType>(req.DecisionType, out var decisionType))
            return Result<MdcDecisionDto>.BadRequest("無効な決定タイプです。");

        if (string.IsNullOrWhiteSpace(req.Content))
            return Result<MdcDecisionDto>.BadRequest("決定内容は必須です。");

        var decision = DomainEntities.MdcDecision.Create(sessionId, decisionType, req.Content, req.DecidedAt, requesterId);

        foreach (var talentId in req.TalentIds ?? [])
            decision.Talents.Add(DomainEntities.MdcDecisionTalent.Create(decision.Id, talentId));

        await repo.AddDecisionAsync(decision, ct);
        await repo.SaveChangesAsync(ct);

        var talents = await GetTalentNamesAsync(decision.Talents.Select(t => t.TalentId), ct);
        return Result<MdcDecisionDto>.Ok(ToDecisionDto(decision, talents, []));
    }

    public async Task<Result<MdcDecisionDto>> UpdateDecisionAsync(Guid requesterId, Guid decisionId, UpdateDecisionRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !SeniorRoles.Contains(requester.Role))
            return Result<MdcDecisionDto>.Forbidden("決定事項を更新する権限がありません。");

        var decision = await repo.FindDecisionAsync(decisionId, ct);
        if (decision is null)
            return Result<MdcDecisionDto>.NotFound("決定事項が見つかりません。");

        if (!Enum.TryParse<DecisionType>(req.DecisionType, out var decisionType))
            return Result<MdcDecisionDto>.BadRequest("無効な決定タイプです。");

        if (string.IsNullOrWhiteSpace(req.Content))
            return Result<MdcDecisionDto>.BadRequest("決定内容は必須です。");

        decision.Update(decisionType, req.Content, req.DecidedAt);

        // タレント紐付けを差し替え
        decision.Talents.Clear();
        foreach (var talentId in req.TalentIds ?? [])
            decision.Talents.Add(DomainEntities.MdcDecisionTalent.Create(decision.Id, talentId));

        await repo.SaveChangesAsync(ct);

        var talents = await GetTalentNamesAsync(decision.Talents.Select(t => t.TalentId), ct);
        var actionItemDtos = decision.ActionItems.Select(a => ToActionItemDto(a, string.Empty)).ToList();
        return Result<MdcDecisionDto>.Ok(ToDecisionDto(decision, talents, actionItemDtos));
    }

    public async Task<Result<MdcActionItemDto>> AddActionItemAsync(Guid requesterId, Guid decisionId, CreateActionItemRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !SeniorRoles.Contains(requester.Role))
            return Result<MdcActionItemDto>.Forbidden("アクションアイテムを追加する権限がありません。");

        var decision = await repo.FindDecisionAsync(decisionId, ct);
        if (decision is null)
            return Result<MdcActionItemDto>.NotFound("決定事項が見つかりません。");

        if (string.IsNullOrWhiteSpace(req.Description))
            return Result<MdcActionItemDto>.BadRequest("内容は必須です。");

        var assignee = await repo.FindUserAsync(req.AssigneeId, ct);
        if (assignee is null)
            return Result<MdcActionItemDto>.NotFound("担当者が見つかりません。");

        var item = DomainEntities.MdcActionItem.Create(decision.SessionId, decisionId, req.Description, req.AssigneeId, req.DueDate);
        await repo.AddActionItemAsync(item, ct);
        await repo.SaveChangesAsync(ct);

        return Result<MdcActionItemDto>.Ok(ToActionItemDto(item, assignee.DisplayName));
    }

    public async Task<Result<IReadOnlyList<MdcActionItemDto>>> GetActionItemsAsync(Guid requesterId, Guid sessionId, MdcActionItemStatus? status, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || (!SeniorRoles.Contains(requester.Role) && requester.Role != UserRole.Manager))
            return Result<IReadOnlyList<MdcActionItemDto>>.Forbidden("アクションアイテムを閲覧する権限がありません。");

        var session = await repo.FindSessionAsync(sessionId, ct);
        if (session is null)
            return Result<IReadOnlyList<MdcActionItemDto>>.NotFound("セッションが見つかりません。");

        var items = await repo.GetActionItemsBySessionAsync(sessionId, status, ct);
        var assigneeIds = items.Select(i => i.AssigneeId).Distinct();
        var users = await repo.GetUsersByIdsAsync(assigneeIds, ct);
        var userMap = users.ToDictionary(u => u.Id, u => u.DisplayName);

        var dtos = items.Select(i => ToActionItemDto(i, userMap.GetValueOrDefault(i.AssigneeId, string.Empty))).ToList();
        return Result<IReadOnlyList<MdcActionItemDto>>.Ok(dtos);
    }

    public async Task<Result<MdcActionItemDto>> UpdateActionItemStatusAsync(Guid requesterId, Guid actionItemId, UpdateActionItemStatusRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<MdcActionItemDto>.Forbidden("権限がありません。");

        var item = await repo.FindActionItemAsync(actionItemId, ct);
        if (item is null)
            return Result<MdcActionItemDto>.NotFound("アクションアイテムが見つかりません。");

        // Senior以上または担当者本人のみ更新可
        if (!SeniorRoles.Contains(requester.Role) && item.AssigneeId != requesterId)
            return Result<MdcActionItemDto>.Forbidden("このアクションアイテムを更新する権限がありません。");

        if (!Enum.TryParse<MdcActionItemStatus>(req.Status, out var newStatus))
            return Result<MdcActionItemDto>.BadRequest("無効なステータスです。");

        item.UpdateStatus(newStatus);
        await repo.SaveChangesAsync(ct);

        var assignee = await repo.FindUserAsync(item.AssigneeId, ct);
        return Result<MdcActionItemDto>.Ok(ToActionItemDto(item, assignee?.DisplayName ?? string.Empty));
    }

    public async Task<Result<IReadOnlyList<MdcDecisionDto>>> GetDecisionsByTalentAsync(Guid requesterId, Guid talentId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<IReadOnlyList<MdcDecisionDto>>.Forbidden("権限がありません。");

        // Senior以上またはManagerは許可（Managerの部下フィルタは今フェーズでは省略）
        if (!SeniorRoles.Contains(requester.Role) && requester.Role != UserRole.Manager)
            return Result<IReadOnlyList<MdcDecisionDto>>.Forbidden("タレントの決定事項を閲覧する権限がありません。");

        var data = await repo.GetDecisionsByTalentAsync(talentId, ct);
        var dtos = data.Select(d => ToDecisionDto(
            d.Decision,
            d.Talents.Select(t => new DecisionTalentDto(t.TalentId, t.TalentName)).ToList(),
            []
        )).ToList();

        return Result<IReadOnlyList<MdcDecisionDto>>.Ok(dtos);
    }

    public async Task<Result<IReadOnlyList<MdcActionItemDto>>> GetActionItemsByUserAsync(Guid requesterId, Guid userId, bool pendingOnly, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<IReadOnlyList<MdcActionItemDto>>.Forbidden("権限がありません。");

        // Senior以上または本人のみ
        if (!SeniorRoles.Contains(requester.Role) && requesterId != userId)
            return Result<IReadOnlyList<MdcActionItemDto>>.Forbidden("他ユーザーのアクションアイテムを閲覧する権限がありません。");

        var data = await repo.GetActionItemsByAssigneeAsync(userId, pendingOnly, ct);
        var dtos = data.Select(d => ToActionItemDto(d.ActionItem, d.AssigneeName)).ToList();
        return Result<IReadOnlyList<MdcActionItemDto>>.Ok(dtos);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<IReadOnlyList<MdcDecisionDto>> ToDecisionDtosAsync(IReadOnlyList<DomainEntities.MdcDecision> decisions, CancellationToken ct)
    {
        var talentIds = decisions.SelectMany(d => d.Talents.Select(t => t.TalentId)).Distinct();
        var assigneeIds = decisions.SelectMany(d => d.ActionItems.Select(a => a.AssigneeId)).Distinct();
        var users = await repo.GetUsersByIdsAsync(talentIds.Concat(assigneeIds).Distinct(), ct);
        var userMap = users.ToDictionary(u => u.Id, u => u.DisplayName);

        return decisions.Select(d =>
        {
            var talents = d.Talents.Select(t => new DecisionTalentDto(t.TalentId, userMap.GetValueOrDefault(t.TalentId, string.Empty))).ToList();
            var actionItems = d.ActionItems.Select(a => ToActionItemDto(a, userMap.GetValueOrDefault(a.AssigneeId, string.Empty))).ToList();
            return ToDecisionDto(d, talents, actionItems);
        }).ToList();
    }

    private async Task<IReadOnlyList<DecisionTalentDto>> GetTalentNamesAsync(IEnumerable<Guid> talentIds, CancellationToken ct)
    {
        var users = await repo.GetUsersByIdsAsync(talentIds, ct);
        return users.Select(u => new DecisionTalentDto(u.Id, u.DisplayName)).ToList();
    }

    private static MdcDecisionDto ToDecisionDto(
        DomainEntities.MdcDecision d,
        IReadOnlyList<DecisionTalentDto> talents,
        IReadOnlyList<MdcActionItemDto> actionItems) =>
        new(d.Id, d.SessionId, d.DecisionType.ToString(), d.Content, d.DecidedAt,
            d.CreatedBy, d.CreatedAt, d.UpdatedAt, talents, actionItems);

    private static MdcActionItemDto ToActionItemDto(DomainEntities.MdcActionItem a, string assigneeName) =>
        new(a.Id, a.SessionId, a.DecisionId, a.Description, a.AssigneeId, assigneeName,
            a.DueDate, a.Status.ToString(), a.CompletedAt, a.CreatedAt, a.UpdatedAt);
}
