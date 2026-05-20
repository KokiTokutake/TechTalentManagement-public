using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Domain.Entities;

public class OneOnOneMeeting
{
    public Guid Id { get; private set; }
    public Guid TalentId { get; private set; }
    public Guid ManagerId { get; private set; }
    public DateOnly ConductedAt { get; private set; }
    public MeetingFormat Format { get; private set; }
    public string? WillTopics { get; private set; }
    public string? CanTopics { get; private set; }
    public string? MustTopics { get; private set; }
    public string? CurrentSituation { get; private set; }
    public string? FreeText { get; private set; }
    public bool ShareWithMdc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public User? Talent { get; private set; }
    public User? Manager { get; private set; }
    public ICollection<MeetingActionItem> ActionItems { get; private set; } = new List<MeetingActionItem>();

    private OneOnOneMeeting() { }

    public static OneOnOneMeeting Create(
        Guid talentId,
        Guid managerId,
        DateOnly conductedAt,
        MeetingFormat format,
        string? willTopics,
        string? canTopics,
        string? mustTopics,
        string? currentSituation,
        string? freeText,
        bool shareWithMdc)
    {
        var now = DateTimeOffset.UtcNow;
        return new OneOnOneMeeting
        {
            Id = Guid.NewGuid(),
            TalentId = talentId,
            ManagerId = managerId,
            ConductedAt = conductedAt,
            Format = format,
            WillTopics = willTopics,
            CanTopics = canTopics,
            MustTopics = mustTopics,
            CurrentSituation = currentSituation,
            FreeText = freeText,
            ShareWithMdc = shareWithMdc,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(
        DateOnly conductedAt,
        MeetingFormat format,
        string? willTopics,
        string? canTopics,
        string? mustTopics,
        string? currentSituation,
        string? freeText,
        bool shareWithMdc)
    {
        ConductedAt = conductedAt;
        Format = format;
        WillTopics = willTopics;
        CanTopics = canTopics;
        MustTopics = mustTopics;
        CurrentSituation = currentSituation;
        FreeText = freeText;
        ShareWithMdc = shareWithMdc;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Delete() => IsDeleted = true;
}
