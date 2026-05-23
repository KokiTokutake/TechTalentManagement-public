namespace TechTalentManagement.Domain.Entities;

public class FeedbackMeeting
{
    public Guid Id { get; private set; }
    public Guid SelfEvaluationId { get; private set; }
    public Guid TalentId { get; private set; }
    public Guid ManagerId { get; private set; }
    public DateOnly ConductedAt { get; private set; }
    public string? OverallComment { get; private set; }
    public string? NextPeriodExpectation { get; private set; }
    public string? TalentReactionNote { get; private set; }
    public bool IsDisclosed { get; private set; }
    public DateTimeOffset? DisclosedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public SelfEvaluation? SelfEvaluation { get; private set; }
    public User? Talent { get; private set; }
    public User? Manager { get; private set; }

    private FeedbackMeeting() { }

    public static FeedbackMeeting Create(Guid selfEvaluationId, Guid talentId, Guid managerId, DateOnly conductedAt, string? overallComment, string? nextPeriodExpectation, string? talentReactionNote)
    {
        var now = DateTimeOffset.UtcNow;
        return new FeedbackMeeting
        {
            Id = Guid.NewGuid(),
            SelfEvaluationId = selfEvaluationId,
            TalentId = talentId,
            ManagerId = managerId,
            ConductedAt = conductedAt,
            OverallComment = overallComment,
            NextPeriodExpectation = nextPeriodExpectation,
            TalentReactionNote = talentReactionNote,
            IsDisclosed = false,
            DisclosedAt = null,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(DateOnly conductedAt, string? overallComment, string? nextPeriodExpectation, string? talentReactionNote)
    {
        ConductedAt = conductedAt;
        OverallComment = overallComment;
        NextPeriodExpectation = nextPeriodExpectation;
        TalentReactionNote = talentReactionNote;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Disclose()
    {
        if (IsDisclosed)
            throw new InvalidOperationException("既に開示済みです。");

        IsDisclosed = true;
        DisclosedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
