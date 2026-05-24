using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Enums;
using DomainEntities = TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Application.GoalKpi;

// ── DTOs ────────────────────────────────────────────────────────────────────

public record GoalKpiDto(
    Guid Id,
    Guid GoalId,
    string MetricName,
    decimal TargetValue,
    string Unit,
    string? MeasurementMethod,
    DateOnly? DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record GoalCheckinDto(
    Guid Id,
    Guid GoalId,
    Guid? KpiId,
    Guid UserId,
    decimal? CurrentValue,
    decimal? ProgressPercent,
    string? Comment,
    DateTimeOffset CheckedInAt);

public record GoalCheckinsResponse(
    Guid GoalId,
    GoalKpiDto? Kpi,
    IReadOnlyList<GoalCheckinDto> Checkins);

// ── Requests ────────────────────────────────────────────────────────────────

public record SaveGoalKpiRequest(
    string MetricName,
    decimal TargetValue,
    string Unit,
    string? MeasurementMethod,
    DateOnly? DueDate);

public record CreateCheckinRequest(
    decimal? CurrentValue,
    string? Comment);

// ── Repository Interface ─────────────────────────────────────────────────────

public interface IGoalKpiRepository
{
    Task<DomainEntities.Goal?> FindGoalAsync(Guid goalId, CancellationToken ct);
    Task<DomainEntities.User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<DomainEntities.EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct);
    Task<DomainEntities.GoalKpi?> FindKpiByGoalAsync(Guid goalId, CancellationToken ct);
    Task<IReadOnlyList<DomainEntities.GoalCheckin>> GetCheckinsAsync(Guid goalId, CancellationToken ct);
    Task AddKpiAsync(DomainEntities.GoalKpi kpi, CancellationToken ct);
    Task AddCheckinAsync(DomainEntities.GoalCheckin checkin, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

// ── Service Interface ────────────────────────────────────────────────────────

public interface IGoalKpiService
{
    Task<Result<GoalKpiDto>> SaveKpiAsync(Guid requesterId, Guid goalId, SaveGoalKpiRequest req, bool isUpdate, CancellationToken ct = default);
    Task<Result<GoalCheckinDto>> CreateCheckinAsync(Guid requesterId, Guid goalId, CreateCheckinRequest req, CancellationToken ct = default);
    Task<Result<GoalCheckinsResponse>> GetCheckinsAsync(Guid requesterId, Guid goalId, CancellationToken ct = default);
}

// ── Service Implementation ───────────────────────────────────────────────────

public class GoalKpiService(IGoalKpiRepository repo) : IGoalKpiService
{
    private static readonly HashSet<UserRole> ManagerRoles = [UserRole.Manager, UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<GoalKpiDto>> SaveKpiAsync(Guid requesterId, Guid goalId, SaveGoalKpiRequest req, bool isUpdate, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<GoalKpiDto>.Forbidden("権限がありません。");

        var goal = await repo.FindGoalAsync(goalId, ct);
        if (goal is null)
            return Result<GoalKpiDto>.NotFound("目標が見つかりません。");

        if (!CanEditGoal(requester, goal))
            return Result<GoalKpiDto>.Forbidden("この目標のKPIを編集する権限がありません。");

        var period = await repo.FindPeriodAsync(goal.EvaluationPeriodId, ct);
        if (period is null || !period.IsGoalRegistrationOpen(DateOnly.FromDateTime(DateTime.UtcNow)))
            return Result<GoalKpiDto>.Forbidden("目標設定期間が終了しています。");

        if (req.TargetValue < 0)
            return Result<GoalKpiDto>.BadRequest("目標値は0以上の値を入力してください。");
        if (string.IsNullOrWhiteSpace(req.MetricName) || req.MetricName.Length > 200)
            return Result<GoalKpiDto>.BadRequest("指標名は1〜200文字で入力してください。");
        if (string.IsNullOrWhiteSpace(req.Unit) || req.Unit.Length > 50)
            return Result<GoalKpiDto>.BadRequest("単位は1〜50文字で入力してください。");

        var existing = await repo.FindKpiByGoalAsync(goalId, ct);

        if (isUpdate)
        {
            if (existing is null)
                return Result<GoalKpiDto>.NotFound("KPIが見つかりません。");
            existing.Update(req.MetricName, req.TargetValue, req.Unit, req.MeasurementMethod, req.DueDate);
            await repo.SaveChangesAsync(ct);
            return Result<GoalKpiDto>.Ok(ToDto(existing));
        }
        else
        {
            if (existing is not null)
                return Result<GoalKpiDto>.Conflict("このKPIは既に存在します。更新する場合はPUTを使用してください。");
            var kpi = DomainEntities.GoalKpi.Create(goalId, req.MetricName, req.TargetValue, req.Unit, req.MeasurementMethod, req.DueDate);
            await repo.AddKpiAsync(kpi, ct);
            await repo.SaveChangesAsync(ct);
            return Result<GoalKpiDto>.Created(ToDto(kpi));
        }
    }

    public async Task<Result<GoalCheckinDto>> CreateCheckinAsync(Guid requesterId, Guid goalId, CreateCheckinRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<GoalCheckinDto>.Forbidden("権限がありません。");

        var goal = await repo.FindGoalAsync(goalId, ct);
        if (goal is null)
            return Result<GoalCheckinDto>.NotFound("目標が見つかりません。");

        if (!CanEditGoal(requester, goal))
            return Result<GoalCheckinDto>.Forbidden("この目標のチェックインを記録する権限がありません。");

        var kpi = await repo.FindKpiByGoalAsync(goalId, ct);

        var checkin = DomainEntities.GoalCheckin.Create(
            goalId, kpi?.Id, requesterId,
            req.CurrentValue, kpi?.TargetValue, req.Comment);
        await repo.AddCheckinAsync(checkin, ct);
        await repo.SaveChangesAsync(ct);

        return Result<GoalCheckinDto>.Created(ToCheckinDto(checkin));
    }

    public async Task<Result<GoalCheckinsResponse>> GetCheckinsAsync(Guid requesterId, Guid goalId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<GoalCheckinsResponse>.Forbidden("権限がありません。");

        var goal = await repo.FindGoalAsync(goalId, ct);
        if (goal is null)
            return Result<GoalCheckinsResponse>.NotFound("目標が見つかりません。");

        if (!CanViewGoal(requester, goal))
            return Result<GoalCheckinsResponse>.Forbidden("この目標を閲覧する権限がありません。");

        var kpi = await repo.FindKpiByGoalAsync(goalId, ct);
        var checkins = await repo.GetCheckinsAsync(goalId, ct);

        var response = new GoalCheckinsResponse(
            goalId,
            kpi is not null ? ToDto(kpi) : null,
            checkins.Select(ToCheckinDto).ToList());

        return Result<GoalCheckinsResponse>.Ok(response);
    }

    private static bool CanEditGoal(DomainEntities.User requester, DomainEntities.Goal goal) =>
        requester.Id == goal.UserId || ManagerRoles.Contains(requester.Role);

    private static bool CanViewGoal(DomainEntities.User requester, DomainEntities.Goal goal) =>
        requester.Id == goal.UserId || ManagerRoles.Contains(requester.Role);

    private static GoalKpiDto ToDto(DomainEntities.GoalKpi kpi) =>
        new(kpi.Id, kpi.GoalId, kpi.MetricName, kpi.TargetValue, kpi.Unit,
            kpi.MeasurementMethod, kpi.DueDate, kpi.CreatedAt, kpi.UpdatedAt);

    private static GoalCheckinDto ToCheckinDto(DomainEntities.GoalCheckin c) =>
        new(c.Id, c.GoalId, c.KpiId, c.UserId, c.CurrentValue, c.ProgressPercent, c.Comment, c.CheckedInAt);
}
