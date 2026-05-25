using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.Succession;

public interface ISuccessionCandidateService
{
    Task<Result<CandidateListResponse>> GetCandidatesAsync(Guid requesterId, Guid positionId, CancellationToken ct = default);
    Task<Result<CandidateDto>> AddCandidateAsync(Guid nominatedBy, Guid positionId, AddCandidateRequest req, CancellationToken ct = default);
    Task<Result<CandidateDto>> UpdateCandidateAsync(Guid requesterId, Guid candidateId, UpdateCandidateRequest req, CancellationToken ct = default);
    Task<Result<bool>> RemoveCandidateAsync(Guid requesterId, Guid candidateId, CancellationToken ct = default);
    Task<Result<SuccessionMapResponse>> GetSuccessionMapAsync(Guid requesterId, CancellationToken ct = default);
    Task<Result<SuggestResponse>> GetSuggestionsAsync(Guid requesterId, Guid periodId, Guid positionId, CancellationToken ct = default);
    Task<Result<DevelopmentActionDto>> AddDevelopmentActionAsync(Guid requesterId, Guid candidateId, AddDevelopmentActionRequest req, CancellationToken ct = default);
    Task<Result<DevelopmentActionDto>> UpdateDevelopmentActionAsync(Guid requesterId, Guid actionId, UpdateDevelopmentActionRequest req, CancellationToken ct = default);
}

public interface ISuccessionCandidateRepository
{
    Task<User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<CriticalPosition?> FindPositionAsync(Guid positionId, CancellationToken ct);
    Task<SuccessionCandidate?> FindCandidateAsync(Guid candidateId, CancellationToken ct);
    Task<SuccessionCandidate?> FindCandidateByPositionAndTalentAsync(Guid positionId, Guid talentId, CancellationToken ct);
    Task<IReadOnlyList<SuccessionCandidate>> GetCandidatesByPositionAsync(Guid positionId, CancellationToken ct);
    Task<IReadOnlyList<SuccessionMapPositionProjection>> GetSuccessionMapAsync(CancellationToken ct);
    Task<IReadOnlyList<NineBoxSuggestionProjection>> GetHighPotentialTalentsAsync(Guid periodId, CancellationToken ct);
    Task<HashSet<Guid>> GetExistingCandidateTalentIdsAsync(Guid positionId, CancellationToken ct);
    Task<SuccessionDevelopmentAction?> FindDevelopmentActionAsync(Guid actionId, CancellationToken ct);
    Task AddCandidateAsync(SuccessionCandidate candidate, CancellationToken ct);
    Task AddDevelopmentActionAsync(SuccessionDevelopmentAction action, CancellationToken ct);
    Task RemoveCandidateAsync(SuccessionCandidate candidate, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class SuccessionCandidateService(ISuccessionCandidateRepository repo) : ISuccessionCandidateService
{
    private static readonly HashSet<UserRole> AllowedRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<CandidateListResponse>> GetCandidatesAsync(Guid requesterId, Guid positionId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !AllowedRoles.Contains(requester.Role))
            return Result<CandidateListResponse>.Forbidden("候補者一覧を閲覧する権限がありません。");

        var position = await repo.FindPositionAsync(positionId, ct);
        if (position is null)
            return Result<CandidateListResponse>.NotFound("指定された重要ポジションが見つかりません。");

        var candidates = await repo.GetCandidatesByPositionAsync(positionId, ct);
        return Result<CandidateListResponse>.Ok(new CandidateListResponse(
            positionId, position.PositionName,
            candidates.Select(ToCandidateDto).ToList()));
    }

    public async Task<Result<CandidateDto>> AddCandidateAsync(Guid nominatedBy, Guid positionId, AddCandidateRequest req, CancellationToken ct = default)
    {
        var nominator = await repo.FindUserAsync(nominatedBy, ct);
        if (nominator is null || !AllowedRoles.Contains(nominator.Role))
            return Result<CandidateDto>.Forbidden("候補者を追加する権限がありません。");

        var position = await repo.FindPositionAsync(positionId, ct);
        if (position is null)
            return Result<CandidateDto>.NotFound("指定された重要ポジションが見つかりません。");

        var existing = await repo.FindCandidateByPositionAndTalentAsync(positionId, req.TalentId, ct);
        if (existing is not null)
            return Result<CandidateDto>.Conflict("このタレントはすでに該当ポジションの後継者候補として登録されています。");

        var candidate = SuccessionCandidate.Create(positionId, req.TalentId, req.Readiness, req.DevelopmentGap, req.DevelopmentActions, nominatedBy);
        await repo.AddCandidateAsync(candidate, ct);
        await repo.SaveChangesAsync(ct);

        // 追加後にナビゲーションプロパティ付きで再取得
        var saved = await repo.FindCandidateAsync(candidate.Id, ct);
        return Result<CandidateDto>.Created(ToCandidateDto(saved!));
    }

    public async Task<Result<CandidateDto>> UpdateCandidateAsync(Guid requesterId, Guid candidateId, UpdateCandidateRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !AllowedRoles.Contains(requester.Role))
            return Result<CandidateDto>.Forbidden("候補者情報を更新する権限がありません。");

        var candidate = await repo.FindCandidateAsync(candidateId, ct);
        if (candidate is null)
            return Result<CandidateDto>.NotFound("指定された後継者候補が見つかりません。");

        candidate.Update(req.Readiness, req.DevelopmentGap, req.DevelopmentActions);
        await repo.SaveChangesAsync(ct);

        return Result<CandidateDto>.Ok(ToCandidateDto(candidate));
    }

    public async Task<Result<bool>> RemoveCandidateAsync(Guid requesterId, Guid candidateId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !AllowedRoles.Contains(requester.Role))
            return Result<bool>.Forbidden("候補者を除外する権限がありません。");

        var candidate = await repo.FindCandidateAsync(candidateId, ct);
        if (candidate is null)
            return Result<bool>.NotFound("指定された後継者候補が見つかりません。");

        await repo.RemoveCandidateAsync(candidate, ct);
        await repo.SaveChangesAsync(ct);

        return Result<bool>.Ok(true);
    }

    public async Task<Result<SuccessionMapResponse>> GetSuccessionMapAsync(Guid requesterId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !AllowedRoles.Contains(requester.Role))
            return Result<SuccessionMapResponse>.Forbidden("サクセッションマップを閲覧する権限がありません。");

        var projections = await repo.GetSuccessionMapAsync(ct);
        var positions = projections.Select(p => new SuccessionMapPositionDto(
            p.Id, p.PositionName, p.Importance.ToString(),
            p.CurrentHolderId is null ? null : new UserRefDto(p.CurrentHolderId.Value, p.CurrentHolderDisplayName!),
            p.ReadyNowCandidates,
            p.ReadySoonCandidates,
            p.ReadyMidTermCandidates,
            HasReadyNowGap: p.Importance == PositionImportance.Critical && p.ReadyNowCandidates.Count == 0
        )).ToList();

        return Result<SuccessionMapResponse>.Ok(new SuccessionMapResponse(positions));
    }

    public async Task<Result<SuggestResponse>> GetSuggestionsAsync(Guid requesterId, Guid periodId, Guid positionId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !AllowedRoles.Contains(requester.Role))
            return Result<SuggestResponse>.Forbidden("サジェスト情報を閲覧する権限がありません。");

        var talents = await repo.GetHighPotentialTalentsAsync(periodId, ct);
        var existingIds = await repo.GetExistingCandidateTalentIdsAsync(positionId, ct);

        var suggestions = talents.Select(t => new SuggestionItem(
            t.TalentId, t.DisplayName,
            t.PotentialLevel.ToString(),
            t.PerformanceLevel.ToString(),
            AlreadyCandidate: existingIds.Contains(t.TalentId)
        )).ToList();

        return Result<SuggestResponse>.Ok(new SuggestResponse(suggestions));
    }

    public async Task<Result<DevelopmentActionDto>> AddDevelopmentActionAsync(Guid requesterId, Guid candidateId, AddDevelopmentActionRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !AllowedRoles.Contains(requester.Role))
            return Result<DevelopmentActionDto>.Forbidden("育成アクションを追加する権限がありません。");

        var candidate = await repo.FindCandidateAsync(candidateId, ct);
        if (candidate is null)
            return Result<DevelopmentActionDto>.NotFound("指定された後継者候補が見つかりません。");

        var action = SuccessionDevelopmentAction.Create(candidateId, req.Action, req.DueDate);
        await repo.AddDevelopmentActionAsync(action, ct);
        await repo.SaveChangesAsync(ct);

        return Result<DevelopmentActionDto>.Created(ToActionDto(action));
    }

    public async Task<Result<DevelopmentActionDto>> UpdateDevelopmentActionAsync(Guid requesterId, Guid actionId, UpdateDevelopmentActionRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !AllowedRoles.Contains(requester.Role))
            return Result<DevelopmentActionDto>.Forbidden("育成アクションを更新する権限がありません。");

        var action = await repo.FindDevelopmentActionAsync(actionId, ct);
        if (action is null)
            return Result<DevelopmentActionDto>.NotFound("指定された育成アクションが見つかりません。");

        action.UpdateStatus(req.Status, req.CompletedAt);
        await repo.SaveChangesAsync(ct);

        return Result<DevelopmentActionDto>.Ok(ToActionDto(action));
    }

    private static CandidateDto ToCandidateDto(SuccessionCandidate c) =>
        new(c.Id,
            new UserRefDto(c.Talent.Id, c.Talent.DisplayName),
            c.Readiness.ToString(),
            c.DevelopmentGap,
            c.DevelopmentActions,
            new UserRefDto(c.Nominator.Id, c.Nominator.DisplayName),
            c.NominatedAt,
            c.UpdatedAt,
            c.DevelopmentActionItems.Select(ToActionDto).ToList());

    private static DevelopmentActionDto ToActionDto(SuccessionDevelopmentAction a) =>
        new(a.Id, a.Action, a.Status.ToString(), a.DueDate?.ToString("yyyy-MM-dd"), a.CompletedAt);
}

// Repository projections
public record SuccessionMapPositionProjection(
    Guid Id,
    string PositionName,
    PositionImportance Importance,
    Guid? CurrentHolderId,
    string? CurrentHolderDisplayName,
    List<MapCandidateItem> ReadyNowCandidates,
    List<MapCandidateItem> ReadySoonCandidates,
    List<MapCandidateItem> ReadyMidTermCandidates
);

public record NineBoxSuggestionProjection(
    Guid TalentId,
    string DisplayName,
    PotentialLevel PotentialLevel,
    PerformanceLevel PerformanceLevel
);

// Response / DTOs
public record CandidateListResponse(
    Guid PositionId,
    string PositionName,
    IReadOnlyList<CandidateDto> Candidates
);

public record CandidateDto(
    Guid Id,
    UserRefDto Talent,
    string Readiness,
    string? DevelopmentGap,
    string? DevelopmentActions,
    UserRefDto NominatedBy,
    DateTimeOffset NominatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<DevelopmentActionDto> DevelopmentActionItems
);

public record DevelopmentActionDto(
    Guid Id,
    string Action,
    string Status,
    string? DueDate,
    DateTimeOffset? CompletedAt
);

public record SuccessionMapResponse(IReadOnlyList<SuccessionMapPositionDto> Positions);

public record SuccessionMapPositionDto(
    Guid Id,
    string PositionName,
    string Importance,
    UserRefDto? CurrentHolder,
    List<MapCandidateItem> ReadyNow,
    List<MapCandidateItem> ReadySoon,
    List<MapCandidateItem> ReadyMidTerm,
    bool HasReadyNowGap
);

public record MapCandidateItem(Guid Id, string DisplayName, Guid CandidateId);

public record SuggestResponse(IReadOnlyList<SuggestionItem> Suggestions);

public record SuggestionItem(
    Guid Id,
    string DisplayName,
    string PotentialLevel,
    string PerformanceLevel,
    bool AlreadyCandidate
);

// Requests
public record AddCandidateRequest(
    Guid TalentId,
    ReadinessLevel Readiness,
    string? DevelopmentGap,
    string? DevelopmentActions
);

public record UpdateCandidateRequest(
    ReadinessLevel Readiness,
    string? DevelopmentGap,
    string? DevelopmentActions
);

public record AddDevelopmentActionRequest(
    string Action,
    DateOnly? DueDate
);

public record UpdateDevelopmentActionRequest(
    SuccessionActionStatus Status,
    DateTimeOffset? CompletedAt
);
