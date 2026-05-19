namespace TeckTalentManagement.Domain.Entities;

public class KeyPerson
{
    public Guid Id { get; private set; }
    public Guid TalentId { get; private set; }
    public Guid DesignatedBy { get; private set; }
    public string? Reason { get; private set; }
    public DateTimeOffset DesignatedAt { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<DevelopmentPlan> DevelopmentPlans { get; private set; } = [];

    private KeyPerson() { }

    public static KeyPerson Create(Guid talentId, Guid designatedBy, string? reason)
    {
        return new KeyPerson
        {
            Id = Guid.NewGuid(),
            TalentId = talentId,
            DesignatedBy = designatedBy,
            Reason = reason,
            DesignatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;
}
