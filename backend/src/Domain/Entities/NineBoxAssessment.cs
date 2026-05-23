using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Domain.Entities;

public class NineBoxAssessment
{
    public Guid Id { get; private set; }
    public Guid TalentId { get; private set; }
    public Guid EvaluationPeriodId { get; private set; }
    public Guid AssessorId { get; private set; }
    public PerformanceLevel PerformanceLevel { get; private set; }
    public PotentialLevel PotentialLevel { get; private set; }
    public string? PerformanceRationale { get; private set; }
    public string? PotentialRationale { get; private set; }
    public DateTimeOffset AssessedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public User Talent { get; private set; } = null!;
    public EvaluationPeriod EvaluationPeriod { get; private set; } = null!;
    public User Assessor { get; private set; } = null!;

    private NineBoxAssessment() { }

    public static NineBoxAssessment Create(
        Guid talentId,
        Guid evaluationPeriodId,
        Guid assessorId,
        PerformanceLevel performanceLevel,
        PotentialLevel potentialLevel,
        string? performanceRationale,
        string? potentialRationale)
    {
        var now = DateTimeOffset.UtcNow;
        return new NineBoxAssessment
        {
            Id = Guid.NewGuid(),
            TalentId = talentId,
            EvaluationPeriodId = evaluationPeriodId,
            AssessorId = assessorId,
            PerformanceLevel = performanceLevel,
            PotentialLevel = potentialLevel,
            PerformanceRationale = performanceRationale,
            PotentialRationale = potentialRationale,
            AssessedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(
        PerformanceLevel performanceLevel,
        PotentialLevel potentialLevel,
        string? performanceRationale,
        string? potentialRationale)
    {
        PerformanceLevel = performanceLevel;
        PotentialLevel = potentialLevel;
        PerformanceRationale = performanceRationale;
        PotentialRationale = potentialRationale;
        AssessedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
