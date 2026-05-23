namespace TechTalentManagement.Application.NineBox.Responses;

public record NineBoxHistoryResponse(
    Guid TalentId,
    string TalentName,
    IReadOnlyList<NineBoxHistoryItem> History);

public record NineBoxHistoryItem(
    Guid PeriodId,
    string PeriodName,
    string PerformanceLevel,
    string PotentialLevel,
    string Label,
    DateTimeOffset AssessedAt);
