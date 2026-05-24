using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.SkillMigration;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class SkillMigrationRepository(AppDbContext db) : ISkillMigrationRepository
{
    public Task<UserRole?> GetUserRoleAsync(Guid userId, CancellationToken ct) =>
        db.Users.Where(u => u.Id == userId).Select(u => (UserRole?)u.Role).FirstOrDefaultAsync(ct);

    public Task<bool> MigrationLogExistsAsync(CancellationToken ct) =>
        db.SkillLevelMigrationLogs.AnyAsync(ct);

    public async Task<IReadOnlyList<LegacySkillLevelDto>> GetLegacySkillsAsync(CancellationToken ct) =>
        await db.UserSkills
            .Include(s => s.User)
            .Include(s => s.SkillItem)
            .Where(s => s.Level <= 5)
            .Select(s => new LegacySkillLevelDto(s.Id, s.User!.DisplayName, s.SkillItem!.Name, s.Level))
            .ToListAsync(ct);

    public async Task UpdateSkillLevelAsync(Guid userSkillId, int newLevel, CancellationToken ct)
    {
        var skill = await db.UserSkills.FindAsync([userSkillId], ct);
        if (skill is not null)
            skill.SetLevelForMigration(newLevel);
    }

    public async Task AddMigrationLogAsync(SkillLevelMigrationLog log, CancellationToken ct) =>
        await db.SkillLevelMigrationLogs.AddAsync(log, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
