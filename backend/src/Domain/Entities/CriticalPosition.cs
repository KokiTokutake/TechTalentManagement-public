using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Domain.Entities;

public class CriticalPosition
{
    public Guid Id { get; private set; }
    public string PositionName { get; private set; } = string.Empty;
    public Guid? CurrentHolderId { get; private set; }
    public PositionImportance Importance { get; private set; }
    public string? RequiredSkills { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public User? CurrentHolder { get; private set; }
    public User CreatedByUser { get; private set; } = null!;
    public ICollection<SuccessionCandidate> Candidates { get; private set; } = [];

    private CriticalPosition() { }

    public static CriticalPosition Create(
        string positionName,
        Guid? currentHolderId,
        PositionImportance importance,
        string? requiredSkills,
        Guid createdBy)
    {
        var now = DateTimeOffset.UtcNow;
        return new CriticalPosition
        {
            Id = Guid.NewGuid(),
            PositionName = positionName,
            CurrentHolderId = currentHolderId,
            Importance = importance,
            RequiredSkills = requiredSkills,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(
        string positionName,
        Guid? currentHolderId,
        PositionImportance importance,
        string? requiredSkills,
        bool isActive)
    {
        PositionName = positionName;
        CurrentHolderId = currentHolderId;
        Importance = importance;
        RequiredSkills = requiredSkills;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
