using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Application.Goals;

public record GoalDto(
    Guid Id,
    Guid UserId,
    Guid EvaluationPeriodId,
    string Category,
    string Title,
    string? Description,
    int DisplayOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record CreateGoalRequest(
    WillCanMust Category,
    string Title,
    string? Description,
    int DisplayOrder
);

public record UpdateGoalRequest(
    string Title,
    string? Description,
    int DisplayOrder
);
