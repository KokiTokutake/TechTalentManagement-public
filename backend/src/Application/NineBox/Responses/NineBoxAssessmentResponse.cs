namespace TeckTalentManagement.Application.NineBox.Responses;

public record NineBoxAssessmentResponse(
    Guid Id,
    Guid TalentId,
    string TalentName,
    Guid PeriodId,
    string PerformanceLevel,
    string PotentialLevel,
    string? PerformanceRationale,
    string? PotentialRationale,
    bool IsHighPotentialCandidate,
    DateTimeOffset AssessedAt);
