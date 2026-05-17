namespace TeckTalentManagement.Application.ManagerEvaluations;

public record ManagerEvaluationDto(
    Guid Id,
    Guid SelfEvaluationId,
    Guid ManagerId,
    int OverallScore,
    string? FeedbackComment,
    bool IsConfirmed,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record SaveManagerDraftRequest(
    int OverallScore,
    string? FeedbackComment
);
