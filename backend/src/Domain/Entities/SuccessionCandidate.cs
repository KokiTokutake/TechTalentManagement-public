using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Domain.Entities;

public class SuccessionCandidate
{
    public Guid Id { get; private set; }
    public Guid PositionId { get; private set; }
    public Guid TalentId { get; private set; }
    public ReadinessLevel Readiness { get; private set; }
    public string? DevelopmentGap { get; private set; }
    public string? DevelopmentActions { get; private set; }
    public Guid NominatedBy { get; private set; }
    public DateTimeOffset NominatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public CriticalPosition Position { get; private set; } = null!;
    public User Talent { get; private set; } = null!;
    public User Nominator { get; private set; } = null!;
    public ICollection<SuccessionDevelopmentAction> DevelopmentActionItems { get; private set; } = [];

    private SuccessionCandidate() { }

    public static SuccessionCandidate Create(
        Guid positionId,
        Guid talentId,
        ReadinessLevel readiness,
        string? developmentGap,
        string? developmentActions,
        Guid nominatedBy)
    {
        var now = DateTimeOffset.UtcNow;
        return new SuccessionCandidate
        {
            Id = Guid.NewGuid(),
            PositionId = positionId,
            TalentId = talentId,
            Readiness = readiness,
            DevelopmentGap = developmentGap,
            DevelopmentActions = developmentActions,
            NominatedBy = nominatedBy,
            NominatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(ReadinessLevel readiness, string? developmentGap, string? developmentActions)
    {
        Readiness = readiness;
        DevelopmentGap = developmentGap;
        DevelopmentActions = developmentActions;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
