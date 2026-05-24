namespace TechTalentManagement.Application.SkillGap;

public record SkillGapItemDto(
    Guid SkillItemId,
    string SkillItemName,
    int SelfLevel,
    int? ManagerLevel,
    int? Gap,
    bool RequiresAttention);

public record CompetencyGapItemDto(
    Guid CompetencyId,
    string CompetencyName,
    string Category,
    int SelfLevel,
    int? ManagerLevel,
    int? Gap,
    bool RequiresAttention);

public record SkillGapResponse(
    Guid TalentId,
    Guid PeriodId,
    int GapThreshold,
    List<SkillGapItemDto> Skills,
    List<CompetencyGapItemDto> Competencies);
