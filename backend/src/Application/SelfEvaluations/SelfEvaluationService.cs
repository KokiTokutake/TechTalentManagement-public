using TechTalentManagement.Application.Common;
using TechTalentManagement.Application.Notification;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.SelfEvaluations;

public interface ISelfEvaluationService
{
    Task<Result<SelfEvaluationDto>> GetAsync(Guid userId, Guid periodId, CancellationToken ct = default);
    Task<Result<SelfEvaluationDto>> SaveDraftAsync(Guid userId, Guid periodId, SaveDraftRequest req, CancellationToken ct = default);
    Task<Result<SelfEvaluationDto>> SubmitAsync(Guid userId, Guid periodId, CancellationToken ct = default);
}

public interface ISelfEvaluationRepository
{
    Task<EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct);
    Task<IReadOnlyList<Goal>> GetGoalsByUserAndPeriodAsync(Guid userId, Guid periodId, CancellationToken ct);
    Task<SelfEvaluation?> FindAsync(Guid userId, Guid periodId, CancellationToken ct);
    Task<User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task AddAsync(SelfEvaluation evaluation, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class SelfEvaluationService(ISelfEvaluationRepository repo, INotificationService notificationService) : ISelfEvaluationService
{
    public async Task<Result<SelfEvaluationDto>> GetAsync(Guid userId, Guid periodId, CancellationToken ct = default)
    {
        var period = await repo.FindPeriodAsync(periodId, ct);
        if (period is null)
            return Result<SelfEvaluationDto>.NotFound("考課期間が見つかりません。");

        var evaluation = await repo.FindAsync(userId, periodId, ct);
        if (evaluation is null)
            return Result<SelfEvaluationDto>.NotFound("自己評価が見つかりません。");

        return Result<SelfEvaluationDto>.Ok(ToDto(evaluation));
    }

    public async Task<Result<SelfEvaluationDto>> SaveDraftAsync(Guid userId, Guid periodId, SaveDraftRequest req, CancellationToken ct = default)
    {
        var period = await repo.FindPeriodAsync(periodId, ct);
        if (period is null)
            return Result<SelfEvaluationDto>.NotFound("考課期間が見つかりません。");

        if (!period.IsEvaluationOpen(DateOnly.FromDateTime(DateTime.UtcNow)))
            return Result<SelfEvaluationDto>.Forbidden("自己評価期間が終了しています。");

        var evaluation = await repo.FindAsync(userId, periodId, ct);
        if (evaluation is null)
        {
            evaluation = SelfEvaluation.Create(userId, periodId);
            await repo.AddAsync(evaluation, ct);
        }

        if (evaluation.IsSubmitted)
            return Result<SelfEvaluationDto>.Conflict("提出済みの自己評価は編集できません。");

        var goals = await repo.GetGoalsByUserAndPeriodAsync(userId, periodId, ct);
        var goalIds = goals.Select(g => g.Id).ToHashSet();

        foreach (var itemReq in req.Items)
        {
            if (!goalIds.Contains(itemReq.GoalId))
                return Result<SelfEvaluationDto>.BadRequest($"目標 {itemReq.GoalId} はこの考課期間に存在しません。");

            var existing = evaluation.Items.FirstOrDefault(i => i.GoalId == itemReq.GoalId);
            if (existing is null)
                evaluation.Items.Add(SelfEvaluationItem.Create(evaluation.Id, itemReq.GoalId, itemReq.AchievementScore, itemReq.Comment));
            else
                existing.Update(itemReq.AchievementScore, itemReq.Comment);
        }

        evaluation.Touch();
        await repo.SaveChangesAsync(ct);

        return Result<SelfEvaluationDto>.Ok(ToDto(evaluation));
    }

    public async Task<Result<SelfEvaluationDto>> SubmitAsync(Guid userId, Guid periodId, CancellationToken ct = default)
    {
        var period = await repo.FindPeriodAsync(periodId, ct);
        if (period is null)
            return Result<SelfEvaluationDto>.NotFound("考課期間が見つかりません。");

        if (!period.IsEvaluationOpen(DateOnly.FromDateTime(DateTime.UtcNow)))
            return Result<SelfEvaluationDto>.Forbidden("自己評価期間が終了しています。");

        var evaluation = await repo.FindAsync(userId, periodId, ct);
        if (evaluation is null)
            return Result<SelfEvaluationDto>.NotFound("自己評価が見つかりません。下書き保存を先に行ってください。");

        if (evaluation.IsSubmitted)
            return Result<SelfEvaluationDto>.Conflict("既に提出済みです。");

        evaluation.Submit();
        await repo.SaveChangesAsync(ct);

        // NT-03: マネージャーに自己評価提出を通知
        var submitter = await repo.FindUserAsync(userId, ct);
        if (submitter?.ManagerId is Guid managerId)
        {
            await notificationService.CreateAsync(
                managerId,
                NotificationType.SelfEvaluationSubmitted,
                "自己評価が提出されました",
                $"{submitter.DisplayName} が自己評価を提出しました。",
                ct: ct);
        }

        return Result<SelfEvaluationDto>.Ok(ToDto(evaluation));
    }

    private static SelfEvaluationDto ToDto(SelfEvaluation e) => new(
        e.Id, e.UserId, e.EvaluationPeriodId,
        e.IsSubmitted, e.SubmittedAt,
        e.CreatedAt, e.UpdatedAt,
        e.Items.Select(i => new SelfEvaluationItemDto(
            i.Id, i.GoalId, i.AchievementScore, i.Comment, i.UpdatedAt
        )).ToList()
    );
}
