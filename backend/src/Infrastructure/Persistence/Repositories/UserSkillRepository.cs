using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Application.UserSkills;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Repositories;

public class UserSkillRepository(AppDbContext db) : IUserSkillRepository
{
    public async Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(Guid userId, CancellationToken ct) =>
        await db.UserSkills
            .Include(s => s.SkillItem)
            .ThenInclude(i => i!.Category)
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.SkillItem!.Category!.DisplayOrder)
            .ThenBy(s => s.SkillItem!.DisplayOrder)
            .ToListAsync(ct);

    public Task<UserSkill?> FindAsync(Guid id, CancellationToken ct) =>
        db.UserSkills
            .Include(s => s.SkillItem)
            .ThenInclude(i => i!.Category)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<bool> ExistsAsync(Guid userId, Guid skillItemId, CancellationToken ct) =>
        db.UserSkills.AnyAsync(s => s.UserId == userId && s.SkillItemId == skillItemId, ct);

    public Task<SkillItem?> FindSkillItemAsync(Guid skillItemId, CancellationToken ct) =>
        db.SkillItems.Include(i => i.Category).FirstOrDefaultAsync(i => i.Id == skillItemId, ct);

    public Task<bool> IsManagerOfAsync(Guid managerId, Guid userId, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Id == userId && u.ManagerId == managerId, ct);

    public async Task AddAsync(UserSkill userSkill, CancellationToken ct) =>
        await db.UserSkills.AddAsync(userSkill, ct);

    public Task DeleteAsync(UserSkill userSkill, CancellationToken ct)
    {
        db.UserSkills.Remove(userSkill);
        return Task.CompletedTask;
    }

    public async Task AddHistoryAsync(UserSkillHistory history, CancellationToken ct) =>
        await db.UserSkillHistories.AddAsync(history, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
