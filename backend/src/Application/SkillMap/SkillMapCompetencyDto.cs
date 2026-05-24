namespace TechTalentManagement.Application.SkillMap;

public record CompetencyAverageDto(string Label, double AvgLevel, int Count);

public record CompetencyHeatmapItemDto(
    Guid CompetencyId,
    string CompetencyName,
    IReadOnlyList<CompetencyAverageDto> Averages);

public record CompetencyCategoryHeatmapDto(
    string Category,
    IReadOnlyList<CompetencyHeatmapItemDto> Competencies);

public record CompetencyHeatmapResponse(
    Guid PeriodId,
    IReadOnlyList<CompetencyCategoryHeatmapDto> Categories);
