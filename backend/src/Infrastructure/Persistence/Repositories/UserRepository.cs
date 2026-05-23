using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.Users;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public interface IUserRepository
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct);
    Task<User?> FindAsync(Guid id, CancellationToken ct);
}

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct) =>
        await db.Users
            .OrderBy(u => u.Role).ThenBy(u => u.DisplayName)
            .Select(u => new UserDto(u.Id, u.DisplayName, u.Email, u.Role.ToString(), u.ManagerId))
            .ToListAsync(ct);

    public Task<User?> FindAsync(Guid id, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
}
