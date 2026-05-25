using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.Succession;

public interface ICriticalPositionService
{
    Task<Result<CriticalPositionListResponse>> GetListAsync(Guid requesterId, bool includeInactive, CancellationToken ct = default);
    Task<Result<CriticalPositionCreatedResponse>> CreateAsync(Guid requesterId, CreateCriticalPositionRequest req, CancellationToken ct = default);
    Task<Result<CriticalPositionDto>> UpdateAsync(Guid requesterId, Guid positionId, UpdateCriticalPositionRequest req, CancellationToken ct = default);
    Task<Result<TalentCandidaciesResponse>> GetTalentCandidaciesAsync(Guid requesterId, Guid talentId, CancellationToken ct = default);
}

public interface ICriticalPositionRepository
{
    Task<User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<CriticalPosition?> FindByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<CriticalPositionListProjection>> GetListAsync(bool includeInactive, CancellationToken ct);
    Task AddAsync(CriticalPosition position, CancellationToken ct);
    Task<IReadOnlyList<TalentCandidacyItem>> GetCandidaciesByTalentIdAsync(Guid talentId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class CriticalPositionService(ICriticalPositionRepository repo) : ICriticalPositionService
{
    private static readonly HashSet<UserRole> AllowedRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<CriticalPositionListResponse>> GetListAsync(Guid requesterId, bool includeInactive, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !AllowedRoles.Contains(requester.Role))
            return Result<CriticalPositionListResponse>.Forbidden("重要ポジション一覧を閲覧する権限がありません。");

        var projections = await repo.GetListAsync(includeInactive, ct);
        var positions = projections.Select(ToDto).ToList();
        return Result<CriticalPositionListResponse>.Ok(new CriticalPositionListResponse(positions));
    }

    public async Task<Result<CriticalPositionCreatedResponse>> CreateAsync(Guid requesterId, CreateCriticalPositionRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !AllowedRoles.Contains(requester.Role))
            return Result<CriticalPositionCreatedResponse>.Forbidden("重要ポジションを登録する権限がありません。");

        var position = CriticalPosition.Create(req.PositionName, req.CurrentHolderId, req.Importance, req.RequiredSkills, requesterId);
        await repo.AddAsync(position, ct);
        await repo.SaveChangesAsync(ct);

        return Result<CriticalPositionCreatedResponse>.Created(
            new CriticalPositionCreatedResponse(position.Id, position.PositionName, position.Importance.ToString(), position.CreatedAt));
    }

    public async Task<Result<CriticalPositionDto>> UpdateAsync(Guid requesterId, Guid positionId, UpdateCriticalPositionRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !AllowedRoles.Contains(requester.Role))
            return Result<CriticalPositionDto>.Forbidden("重要ポジションを更新する権限がありません。");

        var position = await repo.FindByIdAsync(positionId, ct);
        if (position is null)
            return Result<CriticalPositionDto>.NotFound("指定された重要ポジションが見つかりません。");

        position.Update(req.PositionName, req.CurrentHolderId, req.Importance, req.RequiredSkills, req.IsActive);
        await repo.SaveChangesAsync(ct);

        return Result<CriticalPositionDto>.Ok(new CriticalPositionDto(
            position.Id, position.PositionName,
            position.CurrentHolder is null ? null : new UserRefDto(position.CurrentHolder.Id, position.CurrentHolder.DisplayName),
            position.Importance.ToString(),
            position.RequiredSkills, position.IsActive,
            CandidateCount: 0, HasReadyNowCandidate: false,
            position.CreatedAt, position.UpdatedAt));
    }

    public async Task<Result<TalentCandidaciesResponse>> GetTalentCandidaciesAsync(Guid requesterId, Guid talentId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !AllowedRoles.Contains(requester.Role))
            return Result<TalentCandidaciesResponse>.Forbidden("後継者候補情報を閲覧する権限がありません。");

        var items = await repo.GetCandidaciesByTalentIdAsync(talentId, ct);
        return Result<TalentCandidaciesResponse>.Ok(new TalentCandidaciesResponse(items));
    }

    private static CriticalPositionDto ToDto(CriticalPositionListProjection p) =>
        new(p.Id, p.PositionName,
            p.CurrentHolderDisplayName is null ? null : new UserRefDto(p.CurrentHolderId!.Value, p.CurrentHolderDisplayName),
            p.Importance.ToString(),
            p.RequiredSkills, p.IsActive,
            p.CandidateCount, p.HasReadyNowCandidate,
            p.CreatedAt, p.UpdatedAt);
}

// Repository projection
public record CriticalPositionListProjection(
    Guid Id,
    string PositionName,
    Guid? CurrentHolderId,
    string? CurrentHolderDisplayName,
    PositionImportance Importance,
    string? RequiredSkills,
    bool IsActive,
    int CandidateCount,
    bool HasReadyNowCandidate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

// Response / DTOs
public record CriticalPositionListResponse(IReadOnlyList<CriticalPositionDto> Positions);

public record CriticalPositionDto(
    Guid Id,
    string PositionName,
    UserRefDto? CurrentHolder,
    string Importance,
    string? RequiredSkills,
    bool IsActive,
    int CandidateCount,
    bool HasReadyNowCandidate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record CriticalPositionCreatedResponse(
    Guid Id,
    string PositionName,
    string Importance,
    DateTimeOffset CreatedAt
);

public record UserRefDto(Guid Id, string DisplayName);

public record TalentCandidaciesResponse(IReadOnlyList<TalentCandidacyItem> Candidacies);

public record TalentCandidacyItem(
    Guid PositionId,
    string PositionName,
    string Importance,
    string Readiness,
    DateTimeOffset NominatedAt
);

// Requests
public record CreateCriticalPositionRequest(
    string PositionName,
    Guid? CurrentHolderId,
    PositionImportance Importance,
    string? RequiredSkills
);

public record UpdateCriticalPositionRequest(
    string PositionName,
    Guid? CurrentHolderId,
    PositionImportance Importance,
    string? RequiredSkills,
    bool IsActive
);
