namespace TeckTalentManagement.Application.SkillMap;

public record SkillMapUserDto(Guid Id, string DisplayName, string Email);

public record UserLevelDto(Guid UserId, int Level);

public record SkillMapItemDto(
    Guid SkillItemId,
    string SkillItemName,
    int DisplayOrder,
    IReadOnlyList<UserLevelDto> UserLevels
);

public record SkillMapCategoryDto(
    Guid CategoryId,
    string CategoryName,
    int DisplayOrder,
    IReadOnlyList<SkillMapItemDto> Items
);

public record SkillMapDto(
    IReadOnlyList<SkillMapUserDto> Users,
    IReadOnlyList<SkillMapCategoryDto> Categories
);
