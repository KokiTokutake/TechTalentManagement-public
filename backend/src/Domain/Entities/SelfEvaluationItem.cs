namespace TechTalentManagement.Domain.Entities;

public class SelfEvaluationItem
{
    public Guid Id { get; private set; }
    public Guid SelfEvaluationId { get; private set; }
    public Guid GoalId { get; private set; }
    public int AchievementScore { get; private set; } // 1〜5
    public string? Comment { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public SelfEvaluation? SelfEvaluation { get; private set; }
    public Goal? Goal { get; private set; }

    private SelfEvaluationItem() { }

    public static SelfEvaluationItem Create(Guid selfEvaluationId, Guid goalId, int score, string? comment)
    {
        if (score < 1 || score > 5)
            throw new ArgumentOutOfRangeException(nameof(score), "達成度スコアは1〜5で指定してください。");

        return new SelfEvaluationItem
        {
            Id = Guid.NewGuid(),
            SelfEvaluationId = selfEvaluationId,
            GoalId = goalId,
            AchievementScore = score,
            Comment = comment,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(int score, string? comment)
    {
        if (score < 1 || score > 5)
            throw new ArgumentOutOfRangeException(nameof(score), "達成度スコアは1〜5で指定してください。");

        AchievementScore = score;
        Comment = comment;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
