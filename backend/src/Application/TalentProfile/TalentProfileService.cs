using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Enums;
using DomainEntities = TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Application.TalentProfile;

public interface ITalentProfileService
{
    Task<Result<TalentProfileDto>> GetProfileAsync(Guid requesterId, Guid talentId, Guid? periodId, CancellationToken ct = default);
    Task<Result<ManagerCommentDto>> UpsertManagerCommentAsync(Guid managerId, Guid talentId, Guid periodId, UpsertManagerCommentRequest req, CancellationToken ct = default);
    Task<Result<KeyPersonFlagDto>> ToggleKeyPersonFlagAsync(Guid requesterId, Guid talentId, bool isActive, string? reason, CancellationToken ct = default);
}

public interface ITalentProfileRepository
{
    Task<DomainEntities.User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<DomainEntities.EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct);
    Task<DomainEntities.EvaluationPeriod?> FindLatestPeriodAsync(CancellationToken ct);
    Task<DomainEntities.NineBoxAssessment?> FindNineBoxAssessmentAsync(Guid talentId, Guid periodId, CancellationToken ct);
    Task<DomainEntities.NineBoxCellDefinition?> FindNineBoxCellAsync(PerformanceLevel perf, PotentialLevel pot, CancellationToken ct);
    Task<DomainEntities.CareerPlan?> FindCareerPlanAsync(Guid talentId, CancellationToken ct);
    Task<IReadOnlyList<DomainEntities.UserSkill>> GetTopSkillsAsync(Guid talentId, int count, CancellationToken ct);
    Task<IReadOnlyList<DomainEntities.OneOnOneMeeting>> GetRecentMeetingsAsync(Guid talentId, int count, CancellationToken ct);
    Task<DomainEntities.TalentManagerComment?> FindManagerCommentAsync(Guid talentId, Guid periodId, CancellationToken ct);
    Task<DomainEntities.KeyPerson?> FindActiveKeyPersonAsync(Guid talentId, CancellationToken ct);
    Task AddManagerCommentAsync(DomainEntities.TalentManagerComment comment, CancellationToken ct);
    Task AddKeyPersonAsync(DomainEntities.KeyPerson keyPerson, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class TalentProfileService(ITalentProfileRepository repo) : ITalentProfileService
{
    private static readonly HashSet<UserRole> SeniorRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<TalentProfileDto>> GetProfileAsync(
        Guid requesterId, Guid talentId, Guid? periodId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<TalentProfileDto>.Forbidden("ユーザーが見つかりません。");

        var talent = await repo.FindUserAsync(talentId, ct);
        if (talent is null)
            return Result<TalentProfileDto>.NotFound("タレントが見つかりません。");

        if (requester.Role == UserRole.Manager && talent.ManagerId != requesterId)
            return Result<TalentProfileDto>.Forbidden("配下のタレントのみ閲覧できます。");

        if (requester.Role == UserRole.Talent)
            return Result<TalentProfileDto>.Forbidden("タレントはこのAPIを使用できません。");

        DomainEntities.EvaluationPeriod? period;
        if (periodId.HasValue)
        {
            period = await repo.FindPeriodAsync(periodId.Value, ct);
            if (period is null)
                return Result<TalentProfileDto>.NotFound("考課期間が見つかりません。");
        }
        else
        {
            period = await repo.FindLatestPeriodAsync(ct);
            if (period is null)
                return Result<TalentProfileDto>.NotFound("考課期間が登録されていません。");
        }

        var nineBox = await repo.FindNineBoxAssessmentAsync(talentId, period.Id, ct);
        NineBoxSummaryDto? nineBoxDto = null;
        if (nineBox is not null)
        {
            var cell = await repo.FindNineBoxCellAsync(nineBox.PerformanceLevel, nineBox.PotentialLevel, ct);
            nineBoxDto = new NineBoxSummaryDto(
                nineBox.PerformanceLevel.ToString(),
                nineBox.PotentialLevel.ToString(),
                cell?.Label ?? string.Empty,
                cell?.Color ?? "#6b7280",
                nineBox.AssessedAt);
        }

        var careerPlan = await repo.FindCareerPlanAsync(talentId, ct);
        WillCanMustDto? wcmDto = careerPlan is not null
            ? new WillCanMustDto(careerPlan.LongTermGoal, careerPlan.MediumTermGoal, careerPlan.CurrentFocus)
            : null;

        var skills = await repo.GetTopSkillsAsync(talentId, 5, ct);
        var skillDtos = skills.Select(s => new SkillSummaryItemDto(
            s.SkillItem?.Name ?? string.Empty,
            s.SkillItem?.Category?.Name ?? string.Empty,
            s.Level)).ToList();

        var meetings = await repo.GetRecentMeetingsAsync(talentId, 3, ct);
        var meetingDtos = meetings.Select(m => new MeetingSummaryItemDto(
            m.Id,
            m.ConductedAt,
            BuildMeetingNotesSummary(m))).ToList();

        var comment = await repo.FindManagerCommentAsync(talentId, period.Id, ct);
        ManagerCommentDto? commentDto = comment is not null
            ? new ManagerCommentDto(comment.Id, comment.TalentId, comment.PeriodId,
                comment.CurrentChallenges, comment.DevelopmentPolicy, comment.ExpectedRole, comment.UpdatedAt)
            : null;

        var keyPerson = await repo.FindActiveKeyPersonAsync(talentId, ct);

        return Result<TalentProfileDto>.Ok(new TalentProfileDto(
            talent.Id,
            talent.DisplayName,
            talent.Role.ToString(),
            period.Id,
            period.Name,
            keyPerson is not null,
            nineBoxDto,
            wcmDto,
            skillDtos,
            meetingDtos,
            commentDto));
    }

    public async Task<Result<ManagerCommentDto>> UpsertManagerCommentAsync(
        Guid managerId, Guid talentId, Guid periodId, UpsertManagerCommentRequest req, CancellationToken ct = default)
    {
        var manager = await repo.FindUserAsync(managerId, ct);
        if (manager is null || manager.Role != UserRole.Manager)
            return Result<ManagerCommentDto>.Forbidden("Managerのみ育成コメントを保存できます。");

        var talent = await repo.FindUserAsync(talentId, ct);
        if (talent is null)
            return Result<ManagerCommentDto>.NotFound("タレントが見つかりません。");

        if (talent.ManagerId != managerId)
            return Result<ManagerCommentDto>.Forbidden("配下のタレントのコメントのみ保存できます。");

        var period = await repo.FindPeriodAsync(periodId, ct);
        if (period is null)
            return Result<ManagerCommentDto>.NotFound("考課期間が見つかりません。");

        var existing = await repo.FindManagerCommentAsync(talentId, periodId, ct);
        if (existing is not null)
        {
            existing.Update(req.CurrentChallenges, req.DevelopmentPolicy, req.ExpectedRole);
        }
        else
        {
            var newComment = DomainEntities.TalentManagerComment.Create(
                talentId, managerId, periodId,
                req.CurrentChallenges, req.DevelopmentPolicy, req.ExpectedRole);
            await repo.AddManagerCommentAsync(newComment, ct);
            existing = newComment;
        }

        await repo.SaveChangesAsync(ct);

        return Result<ManagerCommentDto>.Ok(new ManagerCommentDto(
            existing.Id, existing.TalentId, existing.PeriodId,
            existing.CurrentChallenges, existing.DevelopmentPolicy, existing.ExpectedRole,
            existing.UpdatedAt));
    }

    public async Task<Result<KeyPersonFlagDto>> ToggleKeyPersonFlagAsync(
        Guid requesterId, Guid talentId, bool isActive, string? reason, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || !SeniorRoles.Contains(requester.Role))
            return Result<KeyPersonFlagDto>.Forbidden("Senior/Admin のみ重点管理フラグを操作できます。");

        var talent = await repo.FindUserAsync(talentId, ct);
        if (talent is null)
            return Result<KeyPersonFlagDto>.NotFound("タレントが見つかりません。");

        var existing = await repo.FindActiveKeyPersonAsync(talentId, ct);

        if (isActive)
        {
            if (existing is not null)
                return Result<KeyPersonFlagDto>.Ok(new KeyPersonFlagDto(talentId, true, existing.DesignatedAt));

            var keyPerson = DomainEntities.KeyPerson.Create(talentId, requesterId, reason);
            await repo.AddKeyPersonAsync(keyPerson, ct);
            await repo.SaveChangesAsync(ct);
            return Result<KeyPersonFlagDto>.Ok(new KeyPersonFlagDto(talentId, true, keyPerson.DesignatedAt));
        }
        else
        {
            if (existing is null)
                return Result<KeyPersonFlagDto>.Ok(new KeyPersonFlagDto(talentId, false, null));

            existing.Deactivate();
            await repo.SaveChangesAsync(ct);
            return Result<KeyPersonFlagDto>.Ok(new KeyPersonFlagDto(talentId, false, null));
        }
    }

    private static string BuildMeetingNotesSummary(DomainEntities.OneOnOneMeeting m)
    {
        var parts = new[] { m.WillTopics, m.CanTopics, m.MustTopics, m.CurrentSituation, m.FreeText }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
        var combined = string.Join(" ", parts);
        return combined.Length > 100 ? combined[..100] + "…" : combined;
    }
}

public record TalentProfileDto(
    Guid TalentId,
    string TalentName,
    string Role,
    Guid PeriodId,
    string PeriodName,
    bool IsKeyPerson,
    NineBoxSummaryDto? NineBox,
    WillCanMustDto? WillCanMust,
    IReadOnlyList<SkillSummaryItemDto> SkillSummary,
    IReadOnlyList<MeetingSummaryItemDto> MeetingSummary,
    ManagerCommentDto? ManagerComment
);

public record NineBoxSummaryDto(
    string PerformanceLevel,
    string PotentialLevel,
    string CellLabel,
    string CellColor,
    DateTimeOffset AssessedAt
);

public record WillCanMustDto(
    string LongTermGoal,
    string MediumTermGoal,
    string CurrentFocus
);

public record SkillSummaryItemDto(
    string SkillName,
    string CategoryName,
    int Level
);

public record MeetingSummaryItemDto(
    Guid Id,
    DateOnly ConductedAt,
    string NotesSummary
);

public record ManagerCommentDto(
    Guid Id,
    Guid TalentId,
    Guid PeriodId,
    string? CurrentChallenges,
    string? DevelopmentPolicy,
    string? ExpectedRole,
    DateTimeOffset UpdatedAt
);

public record KeyPersonFlagDto(
    Guid TalentId,
    bool IsActive,
    DateTimeOffset? UpdatedAt
);

public record UpsertManagerCommentRequest(
    string? CurrentChallenges,
    string? DevelopmentPolicy,
    string? ExpectedRole
);
