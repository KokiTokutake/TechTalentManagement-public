using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.CompetencyMaster;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class CompetencyMasterRepository(AppDbContext db) : ICompetencyMasterRepository
{
    public Task<bool> IsAdminAsync(Guid userId, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Id == userId && u.Role == UserRole.Admin, ct);

    public async Task<IReadOnlyList<CompetencyMaster>> GetActiveAsync(CancellationToken ct) =>
        await db.CompetencyMasters
            .Where(c => c.IsActive)
            .OrderBy(c => c.Category)
            .ThenBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

    public Task<CompetencyMaster?> FindAsync(Guid id, CancellationToken ct) =>
        db.CompetencyMasters.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExistsByCategoryAndNameAsync(CompetencyCategory category, string name, Guid? excludeId, CancellationToken ct) =>
        db.CompetencyMasters.AnyAsync(c =>
            c.Category == category &&
            c.Name == name &&
            (excludeId == null || c.Id != excludeId.Value), ct);

    public async Task AddAsync(CompetencyMaster competency, CancellationToken ct) =>
        await db.CompetencyMasters.AddAsync(competency, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
