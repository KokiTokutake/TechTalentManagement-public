using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.SkillMap;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class SkillMapRepository(AppDbContext db) : ISkillMapRepository
{
    public async Task<UserRole?> GetUserRoleAsync(Guid userId, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user?.Role;
    }

    public async Task<IReadOnlyList<User>> GetTalentUsersAsync(CancellationToken ct) =>
        await db.Users
            .Where(u => u.Role == UserRole.Talent)
            .OrderBy(u => u.DisplayName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<UserSkill>> GetApprovedSkillsForTalentsAsync(CancellationToken ct) =>
        await db.UserSkills
            .Include(s => s.SkillItem)
            .ThenInclude(i => i!.Category)
            .Where(s => s.Status == UserSkillStatus.Approved &&
                        db.Users.Any(u => u.Id == s.UserId && u.Role == UserRole.Talent))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CompetencyHeatmapRawDto>> GetCompetencyHeatmapDataAsync(
        Guid periodId, Guid? departmentId, CancellationToken ct)
    {
        // User エンティティには Department フィールドがないため、Role を集計ラベルとして使用
        var query = db.CompetencyEvaluations
            .Include(e => e.Competency)
            .Include(e => e.Talent)
            .Where(e => e.PeriodId == periodId && e.EvaluatorType == CompetencyEvaluatorType.Self);

        return await query
            .Select(e => new CompetencyHeatmapRawDto(
                e.CompetencyId,
                e.Competency!.Name,
                e.Competency.Category.ToString(),
                e.Talent!.Role.ToString(),
                e.Level))
            .ToListAsync(ct);
    }
}
