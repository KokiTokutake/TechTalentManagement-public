namespace TechTalentManagement.Domain.Entities;

public class CareerPlanComment
{
    public Guid Id { get; private set; }
    public Guid CareerPlanId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public CareerPlan? CareerPlan { get; private set; }
    public User? Author { get; private set; }

    private CareerPlanComment() { }

    public static CareerPlanComment Create(Guid careerPlanId, Guid authorId, string content)
    {
        return new CareerPlanComment
        {
            Id = Guid.NewGuid(),
            CareerPlanId = careerPlanId,
            AuthorId = authorId,
            Content = content,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
