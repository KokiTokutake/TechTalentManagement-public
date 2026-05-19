using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Application.KeyPerson;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Repositories;

public class KeyPersonRepository(AppDbContext db) : IKeyPersonRepository
{
    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FindAsync([userId], ct).AsTask();

    public Task<KeyPerson?> FindActiveByTalentIdAsync(Guid talentId, CancellationToken ct) =>
        db.KeyPersons.FirstOrDefaultAsync(k => k.TalentId == talentId && k.IsActive, ct);

    public Task<KeyPerson?> FindAsync(Guid id, CancellationToken ct) =>
        db.KeyPersons.FindAsync([id], ct).AsTask();

    public async Task<IReadOnlyList<KeyPerson>> GetActiveListAsync(CancellationToken ct) =>
        await db.KeyPersons
            .Where(k => k.IsActive)
            .OrderBy(k => k.DesignatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(KeyPerson keyPerson, CancellationToken ct) =>
        await db.KeyPersons.AddAsync(keyPerson, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
