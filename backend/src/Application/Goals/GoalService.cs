using TeckTalentManagement.Application.Common;
using TeckTalentManagement.Domain.Entities;
using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Application.Goals;

public interface IGoalService
{
    Task<Result<IReadOnlyList<GoalDto>>> GetGoalsAsync(Guid userId, Guid periodId, CancellationToken ct = default);
    Task<Result<GoalDto>> CreateGoalAsync(Guid userId, Guid periodId, CreateGoalRequest req, CancellationToken ct = default);
    Task<Result<GoalDto>> UpdateGoalAsync(Guid userId, Guid goalId, UpdateGoalRequest req, CancellationToken ct = default);
    Task<Result<bool>> DeleteGoalAsync(Guid userId, Guid goalId, CancellationToken ct = default);
}

public interface IGoalRepository
{
    Task<EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct);
    Task<IReadOnlyList<Goal>> GetByUserAndPeriodAsync(Guid userId, Guid periodId, CancellationToken ct);
    Task<Goal?> FindAsync(Guid goalId, CancellationToken ct);
    Task AddAsync(Goal goal, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task DeleteAsync(Goal goal, CancellationToken ct);
}

public class GoalService(IGoalRepository repo) : IGoalService
{
    public async Task<Result<IReadOnlyList<GoalDto>>> GetGoalsAsync(Guid userId, Guid periodId, CancellationToken ct = default)
    {
        var period = await repo.FindPeriodAsync(periodId, ct);
        if (period is null)
            return Result<IReadOnlyList<GoalDto>>.NotFound("考課期間が見つかりません。");

        var goals = await repo.GetByUserAndPeriodAsync(userId, periodId, ct);
        return Result<IReadOnlyList<GoalDto>>.Ok(goals.Select(ToDto).ToList());
    }

    public async Task<Result<GoalDto>> CreateGoalAsync(Guid userId, Guid periodId, CreateGoalRequest req, CancellationToken ct = default)
    {
        var period = await repo.FindPeriodAsync(periodId, ct);
        if (period is null)
            return Result<GoalDto>.NotFound("考課期間が見つかりません。");

        if (!period.IsGoalRegistrationOpen(DateOnly.FromDateTime(DateTime.UtcNow)))
            return Result<GoalDto>.Forbidden("目標登録期間が終了しています。");

        var goal = Goal.Create(userId, periodId, req.Category, req.Title, req.Description, req.DisplayOrder);
        await repo.AddAsync(goal, ct);
        await repo.SaveChangesAsync(ct);

        return Result<GoalDto>.Ok(ToDto(goal));
    }

    public async Task<Result<GoalDto>> UpdateGoalAsync(Guid userId, Guid goalId, UpdateGoalRequest req, CancellationToken ct = default)
    {
        var goal = await repo.FindAsync(goalId, ct);
        if (goal is null)
            return Result<GoalDto>.NotFound("目標が見つかりません。");

        if (goal.UserId != userId)
            return Result<GoalDto>.Forbidden("他のユーザーの目標は編集できません。");

        var period = await repo.FindPeriodAsync(goal.EvaluationPeriodId, ct);
        if (period is null || !period.IsGoalRegistrationOpen(DateOnly.FromDateTime(DateTime.UtcNow)))
            return Result<GoalDto>.Forbidden("目標登録期間が終了しています。");

        goal.Update(req.Title, req.Description, req.DisplayOrder);
        await repo.SaveChangesAsync(ct);

        return Result<GoalDto>.Ok(ToDto(goal));
    }

    public async Task<Result<bool>> DeleteGoalAsync(Guid userId, Guid goalId, CancellationToken ct = default)
    {
        var goal = await repo.FindAsync(goalId, ct);
        if (goal is null)
            return Result<bool>.NotFound("目標が見つかりません。");

        if (goal.UserId != userId)
            return Result<bool>.Forbidden("他のユーザーの目標は削除できません。");

        var period = await repo.FindPeriodAsync(goal.EvaluationPeriodId, ct);
        if (period is null || !period.IsGoalRegistrationOpen(DateOnly.FromDateTime(DateTime.UtcNow)))
            return Result<bool>.Forbidden("目標登録期間が終了しています。");

        await repo.DeleteAsync(goal, ct);
        await repo.SaveChangesAsync(ct);

        return Result<bool>.Ok(true);
    }

    private static GoalDto ToDto(Goal g) => new(
        g.Id, g.UserId, g.EvaluationPeriodId,
        g.Category.ToString(), g.Title, g.Description,
        g.DisplayOrder, g.CreatedAt, g.UpdatedAt
    );
}
