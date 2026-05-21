namespace TeckTalentManagement.Application.NineBox.Responses;

public record NineBoxMapResponse(
    Guid PeriodId,
    string PeriodName,
    IReadOnlyList<NineBoxMapCell> Cells,
    NineBoxMapSummary Summary);

public record NineBoxMapCell(
    string PerformanceLevel,
    string PotentialLevel,
    string Label,
    string Color,
    int Count,
    IReadOnlyList<NineBoxTalentItem> Talents);

public record NineBoxTalentItem(
    Guid AssessmentId,
    Guid TalentId,
    string TalentName,
    string Initials,
    string Role,
    string? PerformanceRationale,
    string? PotentialRationale,
    bool IsHighPotentialCandidate,
    NineBoxPreviousCell? PreviousCell,
    string? Movement);

public record NineBoxPreviousCell(string PerformanceLevel, string PotentialLevel);

public record NineBoxMapSummary(int TotalAssessed, int TotalTalents, int HighPotentialCount);
