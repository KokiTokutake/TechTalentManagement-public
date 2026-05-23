namespace TechTalentManagement.Domain.Entities;

public class DevelopmentPlan
{
    public Guid Id { get; private set; }
    public Guid KeyPersonId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Challenge { get; private set; } = string.Empty;
    public string Goal { get; private set; } = string.Empty;
    public string SupportAction { get; private set; } = string.Empty;
    public DateOnly? DueDate { get; private set; }
    public int Version { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private DevelopmentPlan() { }

    public static DevelopmentPlan Create(Guid keyPersonId, Guid authorId, string challenge, string goal, string supportAction, DateOnly? dueDate, int version)
    {
        return new DevelopmentPlan
        {
            Id = Guid.NewGuid(),
            KeyPersonId = keyPersonId,
            AuthorId = authorId,
            Challenge = challenge,
            Goal = goal,
            SupportAction = supportAction,
            DueDate = dueDate,
            Version = version,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
