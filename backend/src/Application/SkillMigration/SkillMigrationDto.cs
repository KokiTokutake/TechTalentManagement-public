namespace TechTalentManagement.Application.SkillMigration;

public record MigrationPreviewItem(string TalentName, string SkillName, int CurrentLevel, int NewLevel);

public record MigrationPreviewResponse(
    Dictionary<int, int> ConversionRule,
    int AffectedCount,
    List<MigrationPreviewItem> Preview);

public record MigrationExecuteResponse(
    bool Success,
    int AffectedCount,
    Guid MigrationLogId,
    DateTimeOffset ExecutedAt);
