using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Domain.Entities;

public class MeetingActionItem
{
    public Guid Id { get; private set; }
    public Guid MeetingId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public ActionItemStatus Status { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public OneOnOneMeeting? Meeting { get; private set; }

    private MeetingActionItem() { }

    public static MeetingActionItem Create(Guid meetingId, string description)
    {
        return new MeetingActionItem
        {
            Id = Guid.NewGuid(),
            MeetingId = meetingId,
            Description = description,
            Status = ActionItemStatus.Pending,
            CompletedAt = null,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Complete()
    {
        Status = ActionItemStatus.Done;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Reopen()
    {
        Status = ActionItemStatus.Pending;
        CompletedAt = null;
    }
}
