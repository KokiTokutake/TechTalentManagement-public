using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.DevelopmentPlan;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class DevelopmentPlanRepository(AppDbContext db) : IDevelopmentPlanRepository
{
    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FindAsync([userId], ct).AsTask();

    public Task<KeyPerson?> FindKeyPersonAsync(Guid keyPersonId, CancellationToken ct) =>
        db.KeyPersons.FindAsync([keyPersonId], ct).AsTask();

    public Task<bool> IsDirectManagerOfAsync(Guid managerId, Guid talentId, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Id == talentId && u.ManagerId == managerId, ct);

    public async Task<IReadOnlyList<DevelopmentPlan>> GetListAsync(Guid keyPersonId, CancellationToken ct) =>
        await db.DevelopmentPlans
            .Where(p => p.KeyPersonId == keyPersonId)
            .OrderBy(p => p.Version)
            .ToListAsync(ct);

    public async Task<int> GetNextVersionAsync(Guid keyPersonId, CancellationToken ct)
    {
        var maxVersion = await db.DevelopmentPlans
            .Where(p => p.KeyPersonId == keyPersonId)
            .MaxAsync(p => (int?)p.Version, ct);
        return (maxVersion ?? 0) + 1;
    }

    public async Task AddAsync(DevelopmentPlan plan, CancellationToken ct) =>
        await db.DevelopmentPlans.AddAsync(plan, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
