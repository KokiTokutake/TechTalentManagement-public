namespace TechTalentManagement.Application.NineBox.Requests;

public record UpdateNineBoxAssessmentRequest(
    string PerformanceLevel,
    string PotentialLevel,
    string? PerformanceRationale,
    string? PotentialRationale);
