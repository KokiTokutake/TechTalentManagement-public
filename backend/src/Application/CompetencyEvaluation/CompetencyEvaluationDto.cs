namespace TechTalentManagement.Application.CompetencyEvaluation;

public record SaveEvaluationItem(Guid CompetencyId, int Level, string? Comment);

public record SaveCompetencyEvaluationsRequest(
    Guid TalentId,
    Guid PeriodId,
    string EvaluatorType,
    List<SaveEvaluationItem> Evaluations);

public record SaveCompetencyEvaluationsResponse(int SavedCount);

public record EvaluationDetailDto(int Level, string? Comment, DateTimeOffset UpdatedAt);

public record CompetencyEvaluationResultDto(
    Guid CompetencyId,
    string CompetencyName,
    string Category,
    EvaluationDetailDto? SelfEvaluation,
    EvaluationDetailDto? ManagerEvaluation,
    int? Gap);

public record CompetencyEvaluationsResponse(
    Guid TalentId,
    Guid PeriodId,
    List<CompetencyEvaluationResultDto> Evaluations);
