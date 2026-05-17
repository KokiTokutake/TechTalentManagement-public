namespace TeckTalentManagement.Domain.Entities;

public class SelfEvaluation
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid EvaluationPeriodId { get; private set; }
    public bool IsSubmitted { get; private set; }
    public DateTimeOffset? SubmittedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public User? User { get; private set; }
    public EvaluationPeriod? EvaluationPeriod { get; private set; }
    public ICollection<SelfEvaluationItem> Items { get; private set; } = [];
    public ManagerEvaluation? ManagerEvaluation { get; private set; }

    private SelfEvaluation() { }

    public static SelfEvaluation Create(Guid userId, Guid periodId)
    {
        var now = DateTimeOffset.UtcNow;
        return new SelfEvaluation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EvaluationPeriodId = periodId,
            IsSubmitted = false,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Submit()
    {
        if (IsSubmitted)
            throw new InvalidOperationException("既に提出済みです。");

        IsSubmitted = true;
        SubmittedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
