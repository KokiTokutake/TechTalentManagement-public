using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Domain.Entities;

public class CompetencyEvaluation
{
    public Guid Id { get; private set; }
    public Guid TalentId { get; private set; }
    public Guid EvaluatorId { get; private set; }
    public Guid CompetencyId { get; private set; }
    public Guid PeriodId { get; private set; }
    public int Level { get; private set; }
    public string? Comment { get; private set; }
    public CompetencyEvaluatorType EvaluatorType { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public User? Talent { get; private set; }
    public User? Evaluator { get; private set; }
    public CompetencyMaster? Competency { get; private set; }
    public EvaluationPeriod? Period { get; private set; }

    private CompetencyEvaluation() { }

    public static CompetencyEvaluation Create(Guid talentId, Guid evaluatorId, Guid competencyId, Guid periodId, int level, string? comment, CompetencyEvaluatorType evaluatorType)
    {
        var now = DateTimeOffset.UtcNow;
        return new CompetencyEvaluation
        {
            Id = Guid.NewGuid(),
            TalentId = talentId,
            EvaluatorId = evaluatorId,
            CompetencyId = competencyId,
            PeriodId = periodId,
            Level = level,
            Comment = comment,
            EvaluatorType = evaluatorType,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(int level, string? comment)
    {
        Level = level;
        Comment = comment;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
