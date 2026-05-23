namespace TechTalentManagement.Domain.Entities;

public class GoalCheckin
{
    public Guid Id { get; private set; }
    public Guid GoalId { get; private set; }
    public Guid? KpiId { get; private set; }
    public Guid UserId { get; private set; }
    public decimal? CurrentValue { get; private set; }
    public decimal? ProgressPercent { get; private set; }
    public string? Comment { get; private set; }
    public DateTimeOffset CheckedInAt { get; private set; }

    public Goal? Goal { get; private set; }
    public GoalKpi? Kpi { get; private set; }
    public User? User { get; private set; }

    private GoalCheckin() { }

    public static GoalCheckin Create(Guid goalId, Guid? kpiId, Guid userId, decimal? currentValue, decimal? targetValue, string? comment)
    {
        decimal? progressPercent = null;
        if (targetValue.HasValue && targetValue.Value > 0 && currentValue.HasValue)
            progressPercent = Math.Round(currentValue.Value / targetValue.Value * 100m, 2);

        return new GoalCheckin
        {
            Id = Guid.NewGuid(),
            GoalId = goalId,
            KpiId = kpiId,
            UserId = userId,
            CurrentValue = currentValue,
            ProgressPercent = progressPercent,
            Comment = comment,
            CheckedInAt = DateTimeOffset.UtcNow
        };
    }
}
