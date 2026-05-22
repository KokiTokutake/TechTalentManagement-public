namespace TeckTalentManagement.Domain.Entities;

public class TalentManagerComment
{
    public Guid Id { get; private set; }
    public Guid TalentId { get; private set; }
    public Guid ManagerId { get; private set; }
    public Guid PeriodId { get; private set; }
    public string? CurrentChallenges { get; private set; }
    public string? DevelopmentPolicy { get; private set; }
    public string? ExpectedRole { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private TalentManagerComment() { }

    public static TalentManagerComment Create(
        Guid talentId,
        Guid managerId,
        Guid periodId,
        string? currentChallenges,
        string? developmentPolicy,
        string? expectedRole)
    {
        return new TalentManagerComment
        {
            Id = Guid.NewGuid(),
            TalentId = talentId,
            ManagerId = managerId,
            PeriodId = periodId,
            CurrentChallenges = currentChallenges,
            DevelopmentPolicy = developmentPolicy,
            ExpectedRole = expectedRole,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string? currentChallenges, string? developmentPolicy, string? expectedRole)
    {
        CurrentChallenges = currentChallenges;
        DevelopmentPolicy = developmentPolicy;
        ExpectedRole = expectedRole;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
