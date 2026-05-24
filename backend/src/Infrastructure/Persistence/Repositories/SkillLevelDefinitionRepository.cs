using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.SkillLevelDefinition;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class SkillLevelDefinitionRepository(AppDbContext db) : ISkillLevelDefinitionRepository
{
    public Task<bool> IsAdminAsync(Guid userId, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Id == userId && u.Role == UserRole.Admin, ct);

    public Task<bool> SkillItemExistsAsync(Guid skillItemId, CancellationToken ct) =>
        db.SkillItems.AnyAsync(i => i.Id == skillItemId, ct);

    public async Task<IReadOnlyList<SkillLevelDefinition>> GetBySkillItemAsync(Guid skillItemId, CancellationToken ct) =>
        await db.SkillLevelDefinitions
            .Where(d => d.SkillItemId == skillItemId)
            .OrderBy(d => d.Level)
            .ToListAsync(ct);

    public async Task UpsertRangeAsync(IEnumerable<SkillLevelDefinition> definitions, CancellationToken ct)
    {
        foreach (var def in definitions)
        {
            var exists = await db.SkillLevelDefinitions
                .AnyAsync(d => d.SkillItemId == def.SkillItemId && d.Level == def.Level, ct);

            if (exists)
                db.SkillLevelDefinitions.Update(def);
            else
                await db.SkillLevelDefinitions.AddAsync(def, ct);
        }
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
