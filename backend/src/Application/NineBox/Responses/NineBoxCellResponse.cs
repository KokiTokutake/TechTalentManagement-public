namespace TeckTalentManagement.Application.NineBox.Responses;

public record NineBoxCellResponse(
    string PerformanceLevel,
    string PotentialLevel,
    string Label,
    string Description,
    string Color);
