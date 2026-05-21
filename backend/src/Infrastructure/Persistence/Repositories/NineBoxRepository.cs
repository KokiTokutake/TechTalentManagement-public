using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Application.NineBox;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Repositories;

public class NineBoxRepository(AppDbContext db) : INineBoxRepository
{
    public Task<EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct) =>
        db.EvaluationPeriods.FirstOrDefaultAsync(p => p.Id == periodId, ct);

    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task<NineBoxAssessment?> FindAssessmentAsync(Guid assessmentId, CancellationToken ct) =>
        db.NineBoxAssessments.FirstOrDefaultAsync(a => a.Id == assessmentId, ct);

    public Task<NineBoxAssessment?> FindAssessmentByTalentAndPeriodAsync(Guid talentId, Guid periodId, CancellationToken ct) =>
        db.NineBoxAssessments.FirstOrDefaultAsync(a => a.TalentId == talentId && a.EvaluationPeriodId == periodId, ct);

    public async Task<IReadOnlyList<NineBoxCellDefinition>> GetCellDefinitionsAsync(CancellationToken ct) =>
        await db.NineBoxCellDefinitions
            .OrderBy(c => c.PerformanceLevel)
            .ThenBy(c => c.PotentialLevel)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<NineBoxAssessment>> GetAssessmentsWithTalentsForPeriodAsync(Guid periodId, CancellationToken ct) =>
        await db.NineBoxAssessments
            .Include(a => a.Talent)
            .Where(a => a.EvaluationPeriodId == periodId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<NineBoxAssessment>> GetAssessmentsWithUsersForPeriodAsync(Guid periodId, CancellationToken ct) =>
        await db.NineBoxAssessments
            .Include(a => a.Talent)
            .Include(a => a.Assessor)
            .Where(a => a.EvaluationPeriodId == periodId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<NineBoxAssessment>> GetAssessmentsForTalentAsync(Guid talentId, CancellationToken ct) =>
        await db.NineBoxAssessments
            .Include(a => a.EvaluationPeriod)
            .Where(a => a.TalentId == talentId)
            .ToListAsync(ct);

    public Task<int> GetTalentCountAsync(CancellationToken ct) =>
        db.Users.CountAsync(u => u.Role == TeckTalentManagement.Domain.Enums.UserRole.Talent, ct);

    public async Task<IReadOnlyList<User>> GetManagersWithTalentsAsync(CancellationToken ct) =>
        await db.Users
            .Where(u => u.Role == TeckTalentManagement.Domain.Enums.UserRole.Manager)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<KeyPerson>> GetActiveKeyPersonsAsync(CancellationToken ct) =>
        await db.KeyPersons.Where(kp => kp.IsActive).ToListAsync(ct);

    public async Task AddKeyPersonAsync(KeyPerson keyPerson, CancellationToken ct) =>
        await db.KeyPersons.AddAsync(keyPerson, ct);

    public async Task AddAsync(NineBoxAssessment assessment, CancellationToken ct) =>
        await db.NineBoxAssessments.AddAsync(assessment, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
