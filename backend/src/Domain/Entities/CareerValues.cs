namespace TechTalentManagement.Domain.Entities;

public class CareerValues
{
    public Guid Id { get; private set; }
    public Guid TalentId { get; private set; }
    public Guid PeriodId { get; private set; }
    public string? ImportantThings { get; private set; }
    public string? MotivationSituations { get; private set; }
    public string? AvoidSituations { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private CareerValues() { }

    public static CareerValues Create(
        Guid talentId,
        Guid periodId,
        string? importantThings,
        string? motivationSituations,
        string? avoidSituations)
    {
        var now = DateTimeOffset.UtcNow;
        return new CareerValues
        {
            Id = Guid.NewGuid(),
            TalentId = talentId,
            PeriodId = periodId,
            ImportantThings = importantThings,
            MotivationSituations = motivationSituations,
            AvoidSituations = avoidSituations,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string? importantThings, string? motivationSituations, string? avoidSituations)
    {
        ImportantThings = importantThings;
        MotivationSituations = motivationSituations;
        AvoidSituations = avoidSituations;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
