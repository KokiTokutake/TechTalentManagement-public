using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.SkillMaster;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class SkillMasterRepository(AppDbContext db) : ISkillMasterRepository
{
    public async Task<bool> IsAdminAsync(Guid userId, CancellationToken ct) =>
        await db.Users.AnyAsync(u => u.Id == userId && u.Role == UserRole.Admin, ct);

    public async Task<IReadOnlyList<SkillCategory>> GetAllCategoriesAsync(CancellationToken ct) =>
        await db.SkillCategories.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).ToListAsync(ct);

    public Task<SkillCategory?> FindCategoryAsync(Guid id, CancellationToken ct) =>
        db.SkillCategories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddCategoryAsync(SkillCategory category, CancellationToken ct) =>
        await db.SkillCategories.AddAsync(category, ct);

    public Task DeleteCategoryAsync(SkillCategory category, CancellationToken ct)
    {
        db.SkillCategories.Remove(category);
        return Task.CompletedTask;
    }

    public Task<bool> HasItemsAsync(Guid categoryId, CancellationToken ct) =>
        db.SkillItems.AnyAsync(i => i.CategoryId == categoryId, ct);

    public async Task<IReadOnlyList<SkillItem>> GetItemsByCategoryAsync(Guid categoryId, CancellationToken ct) =>
        await db.SkillItems
            .Where(i => i.CategoryId == categoryId)
            .OrderBy(i => i.DisplayOrder).ThenBy(i => i.Name)
            .ToListAsync(ct);

    public Task<SkillItem?> FindItemAsync(Guid id, CancellationToken ct) =>
        db.SkillItems.FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task AddItemAsync(SkillItem item, CancellationToken ct) =>
        await db.SkillItems.AddAsync(item, ct);

    public Task DeleteItemAsync(SkillItem item, CancellationToken ct)
    {
        db.SkillItems.Remove(item);
        return Task.CompletedTask;
    }

    public Task<bool> IsItemInUseAsync(Guid itemId, CancellationToken ct) =>
        db.UserSkills.AnyAsync(s => s.SkillItemId == itemId, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
