using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.CareerValues;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class CareerValuesRepository(AppDbContext db) : ICareerValuesRepository
{
    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task<EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct) =>
        db.EvaluationPeriods.FirstOrDefaultAsync(p => p.Id == periodId, ct);

    public Task<CareerValues?> FindAsync(Guid talentId, Guid periodId, CancellationToken ct) =>
        db.CareerValues.FirstOrDefaultAsync(v => v.TalentId == talentId && v.PeriodId == periodId, ct);

    public async Task AddAsync(CareerValues values, CancellationToken ct) =>
        await db.CareerValues.AddAsync(values, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
