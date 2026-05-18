namespace TeckTalentManagement.Application.CareerPlan;

public record CareerPlanDto(
    Guid Id,
    Guid UserId,
    int PeriodYear,
    string LongTermGoal,
    string MediumTermGoal,
    string CurrentFocus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record SaveCareerPlanRequest(
    string LongTermGoal,
    string MediumTermGoal,
    string CurrentFocus
);

public record IntegratedViewGoalDto(
    Guid Id,
    string Category,
    string Title,
    string? Description,
    int DisplayOrder
);

public record IntegratedViewSkillDto(
    Guid Id,
    Guid SkillItemId,
    string SkillItemName,
    string CategoryName,
    int Level
);

public record IntegratedViewDto(
    CareerPlanDto? Will,
    IReadOnlyList<IntegratedViewSkillDto> Can,
    IReadOnlyList<IntegratedViewGoalDto> Must
);
