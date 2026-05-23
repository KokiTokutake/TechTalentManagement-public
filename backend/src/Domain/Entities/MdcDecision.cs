using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Domain.Entities;

public class MdcDecision
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public DecisionType DecisionType { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateOnly DecidedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public ICollection<MdcDecisionTalent> Talents { get; private set; } = [];
    public ICollection<MdcActionItem> ActionItems { get; private set; } = [];

    private MdcDecision() { }

    public static MdcDecision Create(
        Guid sessionId,
        DecisionType decisionType,
        string content,
        DateOnly decidedAt,
        Guid createdBy)
    {
        return new MdcDecision
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            DecisionType = decisionType,
            Content = content,
            DecidedAt = decidedAt,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(DecisionType decisionType, string content, DateOnly decidedAt)
    {
        DecisionType = decisionType;
        Content = content;
        DecidedAt = decidedAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public class MdcDecisionTalent
{
    public Guid DecisionId { get; private set; }
    public Guid TalentId { get; private set; }

    private MdcDecisionTalent() { }

    public static MdcDecisionTalent Create(Guid decisionId, Guid talentId) =>
        new() { DecisionId = decisionId, TalentId = talentId };
}
