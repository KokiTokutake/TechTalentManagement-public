using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.CommitteeSession;

public interface ICommitteeSessionService
{
    Task<Result<IReadOnlyList<CommitteeSessionDto>>> GetListAsync(Guid requesterId, CancellationToken ct = default);
    Task<Result<CommitteeSessionDto>> CreateAsync(Guid requesterId, CreateCommitteeSessionRequest req, CancellationToken ct = default);
    Task<Result<CommitteeSessionDto>> GetAsync(Guid requesterId, Guid sessionId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CommitteeSummaryItemDto>>> GetCommitteeSummaryAsync(Guid requesterId, CancellationToken ct = default);
}

public interface ICommitteeSessionRepository
{
    Task<Domain.Entities.User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<Domain.Entities.CommitteeSession>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<Domain.Entities.CommitteeSession>> GetListForManagerAsync(Guid managerId, CancellationToken ct);
    Task<Domain.Entities.CommitteeSession?> FindAsync(Guid id, CancellationToken ct);
    Task AddAsync(Domain.Entities.CommitteeSession session, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task<IReadOnlyList<CommitteeSummaryData>> GetCommitteeSummaryDataAsync(CancellationToken ct);
}

public record CommitteeSummaryData(
    Guid KeyPersonId,
    Guid TalentId,
    string TalentName,
    string? LongTermGoal,
    string? CurrentFocus,
    int SkillCount,
    bool HasDevelopmentPlan
);

public class CommitteeSessionService(ICommitteeSessionRepository repo) : ICommitteeSessionService
{
    private static readonly HashSet<UserRole> ManagerRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<IReadOnlyList<CommitteeSessionDto>>> GetListAsync(Guid requesterId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<IReadOnlyList<CommitteeSessionDto>>.Forbidden("セッション一覧を閲覧する権限がありません。");

        IReadOnlyList<Domain.Entities.CommitteeSession> sessions;
        if (ManagerRoles.Contains(requester.Role))
            sessions = await repo.GetAllAsync(ct);
        else if (requester.Role == UserRole.Manager)
            sessions = await repo.GetListForManagerAsync(requesterId, ct);
        else
            return Result<IReadOnlyList<CommitteeSessionDto>>.Forbidden("セッション一覧を閲覧する権限がありません。");

        return Result<IReadOnlyList<CommitteeSessionDto>>.Ok(sessions.Select(ToDto).ToList());
    }

    public async Task<Result<CommitteeSessionDto>> CreateAsync(Guid requesterId, CreateCommitteeSessionRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !ManagerRoles.Contains(requester.Role))
            return Result<CommitteeSessionDto>.Forbidden("委員会セッションを作成する権限がありません。");

        var session = Domain.Entities.CommitteeSession.Create(req.HeldOn, req.Minutes, requesterId);

        foreach (var userId in req.ParticipantIds.Distinct())
            session.Participants.Add(Domain.Entities.CommitteeSessionParticipant.Create(session.Id, userId));

        foreach (var keyPersonId in req.KeyPersonIds.Distinct())
            session.KeyPersons.Add(Domain.Entities.CommitteeSessionKeyPerson.Create(session.Id, keyPersonId));

        await repo.AddAsync(session, ct);
        await repo.SaveChangesAsync(ct);

        return Result<CommitteeSessionDto>.Ok(ToDto(session));
    }

    public async Task<Result<CommitteeSessionDto>> GetAsync(Guid requesterId, Guid sessionId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<CommitteeSessionDto>.Forbidden("セッションを閲覧する権限がありません。");

        var session = await repo.FindAsync(sessionId, ct);
        if (session is null)
            return Result<CommitteeSessionDto>.NotFound("委員会セッションが見つかりません。");

        if (!ManagerRoles.Contains(requester.Role) && requester.Role != UserRole.Manager)
            return Result<CommitteeSessionDto>.Forbidden("セッションを閲覧する権限がありません。");

        return Result<CommitteeSessionDto>.Ok(ToDto(session));
    }

    public async Task<Result<IReadOnlyList<CommitteeSummaryItemDto>>> GetCommitteeSummaryAsync(Guid requesterId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !ManagerRoles.Contains(requester.Role))
            return Result<IReadOnlyList<CommitteeSummaryItemDto>>.Forbidden("委員会サマリーを閲覧する権限がありません。");

        var data = await repo.GetCommitteeSummaryDataAsync(ct);
        var items = data.Select(d => new CommitteeSummaryItemDto(
            d.KeyPersonId, d.TalentId, d.TalentName,
            d.LongTermGoal, d.CurrentFocus,
            d.SkillCount, d.HasDevelopmentPlan
        )).ToList();

        return Result<IReadOnlyList<CommitteeSummaryItemDto>>.Ok(items);
    }

    private static CommitteeSessionDto ToDto(Domain.Entities.CommitteeSession s) => new(
        s.Id, s.HeldOn, s.Minutes, s.CreatedBy, s.CreatedAt,
        s.Participants.Select(p => p.UserId).ToList(),
        s.KeyPersons.Select(k => k.KeyPersonId).ToList()
    );
}

public record CommitteeSessionDto(
    Guid Id,
    DateOnly HeldOn,
    string Minutes,
    Guid CreatedBy,
    DateTimeOffset CreatedAt,
    IReadOnlyList<Guid> ParticipantIds,
    IReadOnlyList<Guid> KeyPersonIds
);

public record CommitteeSummaryItemDto(
    Guid KeyPersonId,
    Guid TalentId,
    string TalentName,
    string? LongTermGoal,
    string? CurrentFocus,
    int SkillCount,
    bool HasDevelopmentPlan
);

public record CreateCommitteeSessionRequest(
    DateOnly HeldOn,
    string Minutes,
    IReadOnlyList<Guid> ParticipantIds,
    IReadOnlyList<Guid> KeyPersonIds
);
