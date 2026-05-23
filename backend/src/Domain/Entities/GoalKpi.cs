namespace TechTalentManagement.Domain.Entities;

public class GoalKpi
{
    public Guid Id { get; private set; }
    public Guid GoalId { get; private set; }
    public string MetricName { get; private set; } = string.Empty;
    public decimal TargetValue { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public string? MeasurementMethod { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Goal? Goal { get; private set; }

    private GoalKpi() { }

    public static GoalKpi Create(Guid goalId, string metricName, decimal targetValue, string unit, string? measurementMethod, DateOnly? dueDate)
    {
        var now = DateTimeOffset.UtcNow;
        return new GoalKpi
        {
            Id = Guid.NewGuid(),
            GoalId = goalId,
            MetricName = metricName,
            TargetValue = targetValue,
            Unit = unit,
            MeasurementMethod = measurementMethod,
            DueDate = dueDate,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string metricName, decimal targetValue, string unit, string? measurementMethod, DateOnly? dueDate)
    {
        MetricName = metricName;
        TargetValue = targetValue;
        Unit = unit;
        MeasurementMethod = measurementMethod;
        DueDate = dueDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
