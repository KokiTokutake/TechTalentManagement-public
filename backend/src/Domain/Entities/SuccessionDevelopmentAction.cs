using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Domain.Entities;

public class SuccessionDevelopmentAction
{
    public Guid Id { get; private set; }
    public Guid CandidateId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public SuccessionActionStatus Status { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public SuccessionCandidate Candidate { get; private set; } = null!;

    private SuccessionDevelopmentAction() { }

    public static SuccessionDevelopmentAction Create(Guid candidateId, string action, DateOnly? dueDate)
    {
        var now = DateTimeOffset.UtcNow;
        return new SuccessionDevelopmentAction
        {
            Id = Guid.NewGuid(),
            CandidateId = candidateId,
            Action = action,
            Status = SuccessionActionStatus.NotStarted,
            DueDate = dueDate,
            CompletedAt = null,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateStatus(SuccessionActionStatus status, DateTimeOffset? completedAt)
    {
        Status = status;
        CompletedAt = completedAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
