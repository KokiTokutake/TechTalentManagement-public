namespace TechTalentManagement.Domain.Entities;

public class CareerPlan
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int PeriodYear { get; private set; }
    public string LongTermGoal { get; private set; } = string.Empty;
    public string MediumTermGoal { get; private set; } = string.Empty;
    public string CurrentFocus { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public User? User { get; private set; }
    public ICollection<CareerPlanComment> Comments { get; private set; } = [];

    private CareerPlan() { }

    public static CareerPlan Create(Guid userId, string longTermGoal, string mediumTermGoal, string currentFocus)
    {
        var now = DateTimeOffset.UtcNow;
        return new CareerPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PeriodYear = now.Year,
            LongTermGoal = longTermGoal,
            MediumTermGoal = mediumTermGoal,
            CurrentFocus = currentFocus,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string longTermGoal, string mediumTermGoal, string currentFocus)
    {
        LongTermGoal = longTermGoal;
        MediumTermGoal = mediumTermGoal;
        CurrentFocus = currentFocus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
