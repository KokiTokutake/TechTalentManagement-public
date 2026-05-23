namespace TechTalentManagement.Application.CareerPlan;

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

public record CareerPlanCommentDto(
    Guid Id,
    Guid CareerPlanId,
    Guid AuthorId,
    string AuthorName,
    string Content,
    DateTimeOffset CreatedAt
);

public record AddCommentRequest(string Content);

public record OrgOverviewItemDto(
    Guid UserId,
    string UserName,
    string? LongTermGoal,
    string? CurrentFocus,
    int? PeriodYear
);
