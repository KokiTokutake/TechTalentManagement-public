using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.SkillGap;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class SkillGapRepository(AppDbContext db) : ISkillGapRepository
{
    public Task<UserRole?> GetUserRoleAsync(Guid userId, CancellationToken ct) =>
        db.Users.Where(u => u.Id == userId).Select(u => (UserRole?)u.Role).FirstOrDefaultAsync(ct);

    public Task<bool> IsManagedByAsync(Guid managerId, Guid talentId, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Id == talentId && u.ManagerId == managerId, ct);

    public async Task<IReadOnlyList<SkillSelfLevelDto>> GetApprovedSkillLevelsAsync(Guid talentId, CancellationToken ct) =>
        await db.UserSkills
            .Include(s => s.SkillItem)
            .Where(s => s.UserId == talentId && s.Status == UserSkillStatus.Approved)
            .Select(s => new SkillSelfLevelDto(s.SkillItemId, s.SkillItem!.Name, s.Level))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CompetencyGapRawDto>> GetCompetencyGapDataAsync(
        Guid talentId, Guid periodId, CancellationToken ct) =>
        await db.CompetencyEvaluations
            .Include(e => e.Competency)
            .Where(e => e.TalentId == talentId && e.PeriodId == periodId)
            .Select(e => new CompetencyGapRawDto(
                e.CompetencyId,
                e.Competency!.Name,
                e.Competency.Category.ToString(),
                e.EvaluatorType,
                e.Level))
            .ToListAsync(ct);
}
