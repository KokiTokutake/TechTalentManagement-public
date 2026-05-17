namespace TeckTalentManagement.Domain.Entities;

public class ManagerEvaluation
{
    public Guid Id { get; private set; }
    public Guid SelfEvaluationId { get; private set; }
    public Guid ManagerId { get; private set; }
    public int OverallScore { get; private set; } // 1〜5
    public string? FeedbackComment { get; private set; }
    public bool IsConfirmed { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public SelfEvaluation? SelfEvaluation { get; private set; }
    public User? Manager { get; private set; }

    private ManagerEvaluation() { }

    public static ManagerEvaluation Create(Guid selfEvaluationId, Guid managerId)
    {
        var now = DateTimeOffset.UtcNow;
        return new ManagerEvaluation
        {
            Id = Guid.NewGuid(),
            SelfEvaluationId = selfEvaluationId,
            ManagerId = managerId,
            OverallScore = 0,
            IsConfirmed = false,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateDraft(int score, string? feedbackComment)
    {
        if (IsConfirmed)
            throw new InvalidOperationException("確定済みの上長評価は編集できません。");
        if (score < 1 || score > 5)
            throw new ArgumentOutOfRangeException(nameof(score), "評価スコアは1〜5で指定してください。");

        OverallScore = score;
        FeedbackComment = feedbackComment;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Confirm()
    {
        if (IsConfirmed)
            throw new InvalidOperationException("既に確定済みです。");
        if (OverallScore == 0)
            throw new InvalidOperationException("スコアを入力してから確定してください。");

        IsConfirmed = true;
        ConfirmedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
