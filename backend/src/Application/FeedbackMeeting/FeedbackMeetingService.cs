using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Enums;
using DomainEntities = TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Application.FeedbackMeeting;

// ── DTOs ────────────────────────────────────────────────────────────────────

public record FeedbackMeetingDto(
    Guid Id,
    Guid SelfEvaluationId,
    Guid TalentId,
    Guid ManagerId,
    DateOnly ConductedAt,
    string? OverallComment,
    string? NextPeriodExpectation,
    string? TalentReactionNote,
    bool IsDisclosed,
    DateTimeOffset? DisclosedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record DisclosedAtDto(Guid Id, bool IsDisclosed, DateTimeOffset? DisclosedAt);

// ── Requests ────────────────────────────────────────────────────────────────

public record CreateFeedbackMeetingRequest(
    Guid SelfEvaluationId,
    Guid TalentId,
    DateOnly ConductedAt,
    string? OverallComment,
    string? NextPeriodExpectation,
    string? TalentReactionNote);

// ── Repository Interface ─────────────────────────────────────────────────────

public interface IFeedbackMeetingRepository
{
    Task<DomainEntities.User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<DomainEntities.SelfEvaluation?> FindSelfEvaluationAsync(Guid selfEvaluationId, CancellationToken ct);
    Task<DomainEntities.EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct);
    Task<DomainEntities.FeedbackMeeting?> FindBySelfEvaluationAsync(Guid selfEvaluationId, CancellationToken ct);
    Task<DomainEntities.FeedbackMeeting?> FindByTalentAndPeriodAsync(Guid talentId, Guid periodId, bool disclosedOnly, CancellationToken ct);
    Task<DomainEntities.FeedbackMeeting?> FindAsync(Guid id, CancellationToken ct);
    Task AddAsync(DomainEntities.FeedbackMeeting meeting, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

// ── Service Interface ────────────────────────────────────────────────────────

public interface IFeedbackMeetingService
{
    Task<Result<FeedbackMeetingDto>> CreateAsync(Guid requesterId, CreateFeedbackMeetingRequest req, CancellationToken ct = default);
    Task<Result<FeedbackMeetingDto>> GetByTalentAndPeriodAsync(Guid requesterId, Guid talentId, Guid periodId, CancellationToken ct = default);
    Task<Result<DisclosedAtDto>> DiscloseAsync(Guid requesterId, Guid meetingId, CancellationToken ct = default);
}

// ── Service Implementation ───────────────────────────────────────────────────

public class FeedbackMeetingService(IFeedbackMeetingRepository repo) : IFeedbackMeetingService
{
    private static readonly HashSet<UserRole> AdminRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<FeedbackMeetingDto>> CreateAsync(Guid requesterId, CreateFeedbackMeetingRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || requester.Role != UserRole.Manager)
            return Result<FeedbackMeetingDto>.Forbidden("フィードバック面談記録を作成できるのは上長のみです。");

        var selfEval = await repo.FindSelfEvaluationAsync(req.SelfEvaluationId, ct);
        if (selfEval is null)
            return Result<FeedbackMeetingDto>.NotFound("自己評価が見つかりません。");

        if (selfEval.UserId != req.TalentId)
            return Result<FeedbackMeetingDto>.BadRequest("自己評価とタレントが一致しません。");

        var talent = await repo.FindUserAsync(req.TalentId, ct);
        if (talent is null)
            return Result<FeedbackMeetingDto>.NotFound("タレントが見つかりません。");

        if (talent.ManagerId != requesterId)
            return Result<FeedbackMeetingDto>.Forbidden("担当する配下タレントのみ面談記録を作成できます。");

        var period = await repo.FindPeriodAsync(selfEval.EvaluationPeriodId, ct);
        if (period is null || !period.IsEvaluationOpen(DateOnly.FromDateTime(DateTime.UtcNow)))
            return Result<FeedbackMeetingDto>.Forbidden("評価期間外のため面談記録を作成できません。");

        var existing = await repo.FindBySelfEvaluationAsync(req.SelfEvaluationId, ct);
        if (existing is not null)
            return Result<FeedbackMeetingDto>.Conflict("この自己評価に対するフィードバック面談記録は既に存在します。");

        var meeting = DomainEntities.FeedbackMeeting.Create(
            req.SelfEvaluationId, req.TalentId, requesterId,
            req.ConductedAt, req.OverallComment, req.NextPeriodExpectation, req.TalentReactionNote);
        await repo.AddAsync(meeting, ct);
        await repo.SaveChangesAsync(ct);

        return Result<FeedbackMeetingDto>.Created(ToDto(meeting));
    }

    public async Task<Result<FeedbackMeetingDto>> GetByTalentAndPeriodAsync(Guid requesterId, Guid talentId, Guid periodId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<FeedbackMeetingDto>.Forbidden("権限がありません。");

        bool isTalent = requester.Id == talentId;
        bool isManager = requester.Role == UserRole.Manager;
        bool isAdmin = AdminRoles.Contains(requester.Role);

        if (!isTalent && !isManager && !isAdmin)
            return Result<FeedbackMeetingDto>.Forbidden("この面談記録を閲覧する権限がありません。");

        // タレントは開示済みのみ取得可能。未開示は 404 で存在を隠蔽
        bool disclosedOnly = isTalent;
        var meeting = await repo.FindByTalentAndPeriodAsync(talentId, periodId, disclosedOnly, ct);
        if (meeting is null)
            return Result<FeedbackMeetingDto>.NotFound("面談記録が見つかりません。");

        return Result<FeedbackMeetingDto>.Ok(ToDto(meeting));
    }

    public async Task<Result<DisclosedAtDto>> DiscloseAsync(Guid requesterId, Guid meetingId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || requester.Role != UserRole.Manager)
            return Result<DisclosedAtDto>.Forbidden("開示操作は上長のみ実行できます。");

        var meeting = await repo.FindAsync(meetingId, ct);
        if (meeting is null)
            return Result<DisclosedAtDto>.NotFound("面談記録が見つかりません。");

        if (meeting.ManagerId != requesterId)
            return Result<DisclosedAtDto>.Forbidden("自分が作成した面談記録のみ開示できます。");

        if (meeting.IsDisclosed)
            return Result<DisclosedAtDto>.Conflict("既に開示済みです。");

        meeting.Disclose();
        await repo.SaveChangesAsync(ct);

        return Result<DisclosedAtDto>.Ok(new(meeting.Id, meeting.IsDisclosed, meeting.DisclosedAt));
    }

    private static FeedbackMeetingDto ToDto(DomainEntities.FeedbackMeeting m) =>
        new(m.Id, m.SelfEvaluationId, m.TalentId, m.ManagerId, m.ConductedAt,
            m.OverallComment, m.NextPeriodExpectation, m.TalentReactionNote,
            m.IsDisclosed, m.DisclosedAt, m.CreatedAt, m.UpdatedAt);
}
