namespace TeckTalentManagement.Application.SelfEvaluations;

public record SelfEvaluationDto(
    Guid Id,
    Guid UserId,
    Guid EvaluationPeriodId,
    bool IsSubmitted,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<SelfEvaluationItemDto> Items
);

public record SelfEvaluationItemDto(
    Guid Id,
    Guid GoalId,
    int AchievementScore,
    string? Comment,
    DateTimeOffset UpdatedAt
);

public record SaveDraftItemRequest(
    Guid GoalId,
    int AchievementScore,
    string? Comment
);

public record SaveDraftRequest(
    IReadOnlyList<SaveDraftItemRequest> Items
);
