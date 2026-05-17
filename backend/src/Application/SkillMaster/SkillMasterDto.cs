namespace TeckTalentManagement.Application.SkillMaster;

public record SkillCategoryDto(
    Guid Id,
    string Name,
    int DisplayOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record SkillItemDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    int DisplayOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CreateSkillCategoryRequest(string Name, int DisplayOrder);
public record UpdateSkillCategoryRequest(string Name, int DisplayOrder);
public record CreateSkillItemRequest(string Name, int DisplayOrder);
public record UpdateSkillItemRequest(string Name, int DisplayOrder);
