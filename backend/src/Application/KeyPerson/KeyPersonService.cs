using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.KeyPerson;

public interface IKeyPersonService
{
    Task<Result<IReadOnlyList<KeyPersonDto>>> GetListAsync(Guid requesterId, CancellationToken ct = default);
    Task<Result<KeyPersonDto>> DesignateAsync(Guid requesterId, DesignateKeyPersonRequest req, CancellationToken ct = default);
    Task<Result<bool>> RevokeAsync(Guid requesterId, Guid keyPersonId, CancellationToken ct = default);
    Task<Result<KeyPersonDto?>> GetMyStatusAsync(Guid talentId, CancellationToken ct = default);
}

public interface IKeyPersonRepository
{
    Task<Domain.Entities.User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<Domain.Entities.KeyPerson?> FindActiveByTalentIdAsync(Guid talentId, CancellationToken ct);
    Task<Domain.Entities.KeyPerson?> FindAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Domain.Entities.KeyPerson>> GetActiveListAsync(CancellationToken ct);
    Task AddAsync(Domain.Entities.KeyPerson keyPerson, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class KeyPersonService(IKeyPersonRepository repo) : IKeyPersonService
{
    private static readonly HashSet<UserRole> ManagerRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<IReadOnlyList<KeyPersonDto>>> GetListAsync(Guid requesterId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !ManagerRoles.Contains(requester.Role))
            return Result<IReadOnlyList<KeyPersonDto>>.Forbidden("重点管理人物一覧を閲覧する権限がありません。");

        var list = await repo.GetActiveListAsync(ct);
        return Result<IReadOnlyList<KeyPersonDto>>.Ok(list.Select(ToDto).ToList());
    }

    public async Task<Result<KeyPersonDto>> DesignateAsync(Guid requesterId, DesignateKeyPersonRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !ManagerRoles.Contains(requester.Role))
            return Result<KeyPersonDto>.Forbidden("重点管理人物を指定する権限がありません。");

        var talent = await repo.FindUserAsync(req.TalentId, ct);
        if (talent is null)
            return Result<KeyPersonDto>.NotFound("指定するタレントが見つかりません。");

        var existing = await repo.FindActiveByTalentIdAsync(req.TalentId, ct);
        if (existing is not null)
            return Result<KeyPersonDto>.Conflict("このタレントはすでに重点管理人物に指定されています。");

        var keyPerson = Domain.Entities.KeyPerson.Create(req.TalentId, requesterId, req.Reason);
        await repo.AddAsync(keyPerson, ct);
        await repo.SaveChangesAsync(ct);

        return Result<KeyPersonDto>.Ok(ToDto(keyPerson));
    }

    public async Task<Result<bool>> RevokeAsync(Guid requesterId, Guid keyPersonId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !ManagerRoles.Contains(requester.Role))
            return Result<bool>.Forbidden("重点管理人物の指定を解除する権限がありません。");

        var keyPerson = await repo.FindAsync(keyPersonId, ct);
        if (keyPerson is null || !keyPerson.IsActive)
            return Result<bool>.NotFound("重点管理人物が見つかりません。");

        keyPerson.Deactivate();
        await repo.SaveChangesAsync(ct);

        return Result<bool>.Ok(true);
    }

    public async Task<Result<KeyPersonDto?>> GetMyStatusAsync(Guid talentId, CancellationToken ct = default)
    {
        var keyPerson = await repo.FindActiveByTalentIdAsync(talentId, ct);
        return Result<KeyPersonDto?>.Ok(keyPerson is not null ? ToDto(keyPerson) : null);
    }

    private static KeyPersonDto ToDto(Domain.Entities.KeyPerson k) =>
        new(k.Id, k.TalentId, k.DesignatedBy, k.Reason, k.DesignatedAt, k.IsActive);
}

public record KeyPersonDto(
    Guid Id,
    Guid TalentId,
    Guid DesignatedBy,
    string? Reason,
    DateTimeOffset DesignatedAt,
    bool IsActive
);

public record DesignateKeyPersonRequest(Guid TalentId, string? Reason);
