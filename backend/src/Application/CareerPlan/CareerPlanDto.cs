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
