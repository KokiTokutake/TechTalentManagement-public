namespace TechTalentManagement.Application.SkillLevelDefinition;

public record SkillLevelDefinitionDto(
    int Level,
    string Description,
    string? Examples,
    bool IsFallback);

public record SkillLevelDefinitionsResponse(
    Guid SkillItemId,
    IReadOnlyList<SkillLevelDefinitionDto> LevelDefinitions);

public record SaveLevelDefinitionItem(
    int Level,
    string Description,
    string? Examples);

public record SaveSkillLevelDefinitionsRequest(
    IReadOnlyList<SaveLevelDefinitionItem> LevelDefinitions);
