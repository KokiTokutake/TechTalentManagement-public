using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.SkillMap;

public interface ISkillMapService
{
    Task<Result<SkillMapDto>> GetSkillMapAsync(Guid requesterId, CancellationToken ct = default);
}

public interface ISkillMapRepository
{
    Task<UserRole?> GetUserRoleAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<User>> GetTalentUsersAsync(CancellationToken ct);
    Task<IReadOnlyList<UserSkill>> GetApprovedSkillsForTalentsAsync(CancellationToken ct);
}

public class SkillMapService(ISkillMapRepository repo) : ISkillMapService
{
    private static readonly HashSet<UserRole> AllowedRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<SkillMapDto>> GetSkillMapAsync(Guid requesterId, CancellationToken ct = default)
    {
        var role = await repo.GetUserRoleAsync(requesterId, ct);
        if (role is null || !AllowedRoles.Contains(role.Value))
            return Result<SkillMapDto>.Forbidden("管理職以上のみ組織スキルマップを閲覧できます。");

        var talentUsers = await repo.GetTalentUsersAsync(ct);
        var approvedSkills = await repo.GetApprovedSkillsForTalentsAsync(ct);

        var users = talentUsers
            .Select(u => new SkillMapUserDto(u.Id, u.DisplayName, u.Email))
            .ToList();

        var categories = approvedSkills
            .GroupBy(s => s.SkillItem!.Category!, (cat, skills) => new SkillMapCategoryDto(
                cat.Id,
                cat.Name,
                cat.DisplayOrder,
                skills
                    .GroupBy(s => s.SkillItem!, (item, itemSkills) => new SkillMapItemDto(
                        item.Id,
                        item.Name,
                        item.DisplayOrder,
                        itemSkills.Select(s => new UserLevelDto(s.UserId, s.Level)).ToList()
                    ))
                    .OrderBy(i => i.DisplayOrder).ThenBy(i => i.SkillItemName)
                    .ToList()
            ))
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.CategoryName)
            .ToList();

        return Result<SkillMapDto>.Ok(new SkillMapDto(users, categories));
    }
}
