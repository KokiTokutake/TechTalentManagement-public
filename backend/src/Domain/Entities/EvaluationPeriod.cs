namespace TeckTalentManagement.Domain.Entities;

public class EvaluationPeriod
{
    public Guid Id { get; private set; }
    public int Year { get; private set; }
    public string Name { get; private set; } = string.Empty; // 例: "2026年度通期考課"
    public DateOnly GoalStartDate { get; private set; }
    public DateOnly GoalEndDate { get; private set; }
    public DateOnly EvaluationStartDate { get; private set; }
    public DateOnly EvaluationEndDate { get; private set; }
    public DateOnly ApprovalDeadline { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<Goal> Goals { get; private set; } = [];
    public ICollection<SelfEvaluation> SelfEvaluations { get; private set; } = [];

    private EvaluationPeriod() { }

    public static EvaluationPeriod Create(
        int year, string name,
        DateOnly goalStart, DateOnly goalEnd,
        DateOnly evalStart, DateOnly evalEnd,
        DateOnly approvalDeadline)
    {
        return new EvaluationPeriod
        {
            Id = Guid.NewGuid(),
            Year = year,
            Name = name,
            GoalStartDate = goalStart,
            GoalEndDate = goalEnd,
            EvaluationStartDate = evalStart,
            EvaluationEndDate = evalEnd,
            ApprovalDeadline = approvalDeadline,
            IsActive = true
        };
    }

    public bool IsGoalRegistrationOpen(DateOnly today) =>
        today >= GoalStartDate && today <= GoalEndDate;

    public bool IsEvaluationOpen(DateOnly today) =>
        today >= EvaluationStartDate && today <= EvaluationEndDate;
}
