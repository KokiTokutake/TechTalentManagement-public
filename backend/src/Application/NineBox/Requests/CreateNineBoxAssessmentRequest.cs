namespace TechTalentManagement.Application.NineBox.Requests;

public record CreateNineBoxAssessmentRequest(
    Guid TalentId,
    Guid PeriodId,
    string PerformanceLevel,
    string PotentialLevel,
    string? PerformanceRationale,
    string? PotentialRationale);
