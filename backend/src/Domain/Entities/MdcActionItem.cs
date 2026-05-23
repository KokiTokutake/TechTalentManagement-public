using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Domain.Entities;

public class MdcActionItem
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid? DecisionId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid AssigneeId { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public MdcActionItemStatus Status { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private MdcActionItem() { }

    public static MdcActionItem Create(
        Guid sessionId,
        Guid? decisionId,
        string description,
        Guid assigneeId,
        DateOnly? dueDate)
    {
        return new MdcActionItem
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            DecisionId = decisionId,
            Description = description,
            AssigneeId = assigneeId,
            DueDate = dueDate,
            Status = MdcActionItemStatus.NotStarted,
            CompletedAt = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateStatus(MdcActionItemStatus status)
    {
        Status = status;
        CompletedAt = (status == MdcActionItemStatus.Done || status == MdcActionItemStatus.Cancelled)
            ? DateTimeOffset.UtcNow
            : null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
