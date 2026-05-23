using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Domain.Entities;

public class Goal
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid EvaluationPeriodId { get; private set; }
    public WillCanMust Category { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public User? User { get; private set; }
    public EvaluationPeriod? EvaluationPeriod { get; private set; }
    public SelfEvaluationItem? SelfEvaluationItem { get; private set; }
    public GoalKpi? GoalKpi { get; private set; }
    public ICollection<GoalCheckin> GoalCheckins { get; private set; } = [];

    private Goal() { }

    public static Goal Create(Guid userId, Guid periodId, WillCanMust category, string title, string? description, int displayOrder)
    {
        var now = DateTimeOffset.UtcNow;
        return new Goal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EvaluationPeriodId = periodId,
            Category = category,
            Title = title,
            Description = description,
            DisplayOrder = displayOrder,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string title, string? description, int displayOrder)
    {
        Title = title;
        Description = description;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
