using TeckTalentManagement.Application.Common;
using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Application.DevelopmentPlan;

public interface IDevelopmentPlanService
{
    Task<Result<IReadOnlyList<DevelopmentPlanDto>>> GetListAsync(Guid requesterId, Guid keyPersonId, CancellationToken ct = default);
    Task<Result<DevelopmentPlanDto>> AddAsync(Guid requesterId, Guid keyPersonId, SaveDevelopmentPlanRequest req, CancellationToken ct = default);
}

public interface IDevelopmentPlanRepository
{
    Task<Domain.Entities.User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<Domain.Entities.KeyPerson?> FindKeyPersonAsync(Guid keyPersonId, CancellationToken ct);
    Task<bool> IsDirectManagerOfAsync(Guid managerId, Guid talentId, CancellationToken ct);
    Task<IReadOnlyList<Domain.Entities.DevelopmentPlan>> GetListAsync(Guid keyPersonId, CancellationToken ct);
    Task<int> GetNextVersionAsync(Guid keyPersonId, CancellationToken ct);
    Task AddAsync(Domain.Entities.DevelopmentPlan plan, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class DevelopmentPlanService(IDevelopmentPlanRepository repo) : IDevelopmentPlanService
{
    private static readonly HashSet<UserRole> ViewerRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<IReadOnlyList<DevelopmentPlanDto>>> GetListAsync(Guid requesterId, Guid keyPersonId, CancellationToken ct = default)
    {
        var keyPerson = await repo.FindKeyPersonAsync(keyPersonId, ct);
        if (keyPerson is null)
            return Result<IReadOnlyList<DevelopmentPlanDto>>.NotFound("重点管理人物が見つかりません。");

        if (!await CanReadAsync(requesterId, keyPerson.TalentId, ct))
            return Result<IReadOnlyList<DevelopmentPlanDto>>.Forbidden("育成方針を閲覧する権限がありません。");

        var plans = await repo.GetListAsync(keyPersonId, ct);
        return Result<IReadOnlyList<DevelopmentPlanDto>>.Ok(plans.Select(ToDto).ToList());
    }

    public async Task<Result<DevelopmentPlanDto>> AddAsync(Guid requesterId, Guid keyPersonId, SaveDevelopmentPlanRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Challenge) && string.IsNullOrWhiteSpace(req.Goal) && string.IsNullOrWhiteSpace(req.SupportAction))
            return Result<DevelopmentPlanDto>.BadRequest("課題・目標・支援アクションのいずれかを入力してください。");

        var keyPerson = await repo.FindKeyPersonAsync(keyPersonId, ct);
        if (keyPerson is null)
            return Result<DevelopmentPlanDto>.NotFound("重点管理人物が見つかりません。");

        if (!keyPerson.IsActive)
            return Result<DevelopmentPlanDto>.Conflict("解除済みの重点管理人物には育成方針を追加できません。");

        if (!await repo.IsDirectManagerOfAsync(requesterId, keyPerson.TalentId, ct))
            return Result<DevelopmentPlanDto>.Forbidden("直属上長のみ育成方針を登録できます。");

        var nextVersion = await repo.GetNextVersionAsync(keyPersonId, ct);
        var plan = Domain.Entities.DevelopmentPlan.Create(
            keyPersonId, requesterId,
            req.Challenge, req.Goal, req.SupportAction,
            req.DueDate, nextVersion
        );
        await repo.AddAsync(plan, ct);
        await repo.SaveChangesAsync(ct);

        return Result<DevelopmentPlanDto>.Ok(ToDto(plan));
    }

    private async Task<bool> CanReadAsync(Guid requesterId, Guid talentId, CancellationToken ct)
    {
        if (requesterId == talentId) return true;
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null) return false;
        if (ViewerRoles.Contains(requester.Role)) return true;
        if (requester.Role == UserRole.Manager)
            return await repo.IsDirectManagerOfAsync(requesterId, talentId, ct);
        return false;
    }

    private static DevelopmentPlanDto ToDto(Domain.Entities.DevelopmentPlan p) =>
        new(p.Id, p.KeyPersonId, p.AuthorId, p.Challenge, p.Goal, p.SupportAction, p.DueDate, p.Version, p.CreatedAt);
}

public record DevelopmentPlanDto(
    Guid Id,
    Guid KeyPersonId,
    Guid AuthorId,
    string Challenge,
    string Goal,
    string SupportAction,
    DateOnly? DueDate,
    int Version,
    DateTimeOffset CreatedAt
);

public record SaveDevelopmentPlanRequest(
    string Challenge,
    string Goal,
    string SupportAction,
    DateOnly? DueDate
);
