using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.SkillMap;

public interface ISkillMapService
{
    Task<Result<SkillMapDto>> GetSkillMapAsync(Guid requesterId, CancellationToken ct = default);
    Task<Result<CompetencyHeatmapResponse>> GetCompetencyHeatmapAsync(Guid requesterId, Guid periodId, Guid? departmentId, CancellationToken ct = default);
}

public interface ISkillMapRepository
{
    Task<UserRole?> GetUserRoleAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<User>> GetTalentUsersAsync(CancellationToken ct);
    Task<IReadOnlyList<UserSkill>> GetApprovedSkillsForTalentsAsync(CancellationToken ct);
    Task<IReadOnlyList<CompetencyHeatmapRawDto>> GetCompetencyHeatmapDataAsync(Guid periodId, Guid? departmentId, CancellationToken ct);
}

public record CompetencyHeatmapRawDto(
    Guid CompetencyId,
    string CompetencyName,
    string Category,
    string TalentDepartment,
    int Level);

public class SkillMapService(ISkillMapRepository repo) : ISkillMapService
{
    private static readonly HashSet<UserRole> AllowedRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];
    private static readonly HashSet<UserRole> CompetencyAllowedRoles = [UserRole.Manager, UserRole.Senior, UserRole.Executive, UserRole.Admin];

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

    public async Task<Result<CompetencyHeatmapResponse>> GetCompetencyHeatmapAsync(
        Guid requesterId,
        Guid periodId,
        Guid? departmentId,
        CancellationToken ct = default)
    {
        var role = await repo.GetUserRoleAsync(requesterId, ct);
        if (role is null || !CompetencyAllowedRoles.Contains(role.Value))
            return Result<CompetencyHeatmapResponse>.Forbidden("管理職以上のみコンピテンシーヒートマップを閲覧できます。");

        var rawData = await repo.GetCompetencyHeatmapDataAsync(periodId, departmentId, ct);

        var categories = rawData
            .GroupBy(d => d.Category)
            .Select(catGroup => new CompetencyCategoryHeatmapDto(
                catGroup.Key,
                catGroup
                    .GroupBy(d => d.CompetencyId)
                    .Select(compGroup =>
                    {
                        var first = compGroup.First();
                        var averages = compGroup
                            .GroupBy(d => d.TalentDepartment)
                            .Select(deptGroup => new CompetencyAverageDto(
                                deptGroup.Key,
                                Math.Round(deptGroup.Average(d => d.Level), 1),
                                deptGroup.Count()))
                            .OrderBy(a => a.Label)
                            .ToList();
                        return new CompetencyHeatmapItemDto(first.CompetencyId, first.CompetencyName, averages);
                    })
                    .ToList()))
            .ToList();

        return Result<CompetencyHeatmapResponse>.Ok(new CompetencyHeatmapResponse(periodId, categories));
    }
}
