using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Domain.Entities;

public class WillCanMustEntry
{
    public Guid Id { get; private set; }
    public Guid TalentId { get; private set; }
    public Guid PeriodId { get; private set; }
    public WcmCategory Category { get; private set; }
    public WcmTimeHorizon? TimeHorizon { get; private set; }
    public CanType? CanType { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public Guid? LinkedSkillItemId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private WillCanMustEntry() { }

    public static WillCanMustEntry Create(
        Guid talentId,
        Guid periodId,
        WcmCategory category,
        string content,
        int displayOrder,
        WcmTimeHorizon? timeHorizon = null,
        Enums.CanType? canType = null,
        Guid? linkedSkillItemId = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new WillCanMustEntry
        {
            Id = Guid.NewGuid(),
            TalentId = talentId,
            PeriodId = periodId,
            Category = category,
            TimeHorizon = timeHorizon,
            CanType = canType,
            Content = content,
            DisplayOrder = displayOrder,
            LinkedSkillItemId = linkedSkillItemId,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
