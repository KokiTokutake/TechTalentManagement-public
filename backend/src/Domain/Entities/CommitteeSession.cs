namespace TechTalentManagement.Domain.Entities;

public class CommitteeSession
{
    public Guid Id { get; private set; }
    public DateOnly HeldOn { get; private set; }
    public string Minutes { get; private set; } = string.Empty;
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public ICollection<CommitteeSessionParticipant> Participants { get; private set; } = [];
    public ICollection<CommitteeSessionKeyPerson> KeyPersons { get; private set; } = [];

    private CommitteeSession() { }

    public static CommitteeSession Create(DateOnly heldOn, string minutes, Guid createdBy)
    {
        return new CommitteeSession
        {
            Id = Guid.NewGuid(),
            HeldOn = heldOn,
            Minutes = minutes,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

public class CommitteeSessionParticipant
{
    public Guid SessionId { get; private set; }
    public Guid UserId { get; private set; }

    private CommitteeSessionParticipant() { }

    public static CommitteeSessionParticipant Create(Guid sessionId, Guid userId) =>
        new() { SessionId = sessionId, UserId = userId };
}

public class CommitteeSessionKeyPerson
{
    public Guid SessionId { get; private set; }
    public Guid KeyPersonId { get; private set; }

    private CommitteeSessionKeyPerson() { }

    public static CommitteeSessionKeyPerson Create(Guid sessionId, Guid keyPersonId) =>
        new() { SessionId = sessionId, KeyPersonId = keyPersonId };
}
