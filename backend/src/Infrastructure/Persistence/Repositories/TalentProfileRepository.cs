using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Application.TalentProfile;
using TeckTalentManagement.Domain.Entities;
using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Infrastructure.Persistence.Repositories;

public class TalentProfileRepository(AppDbContext db) : ITalentProfileRepository
{
    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task<EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct) =>
        db.EvaluationPeriods.FirstOrDefaultAsync(p => p.Id == periodId, ct);

    public Task<EvaluationPeriod?> FindLatestPeriodAsync(CancellationToken ct) =>
        db.EvaluationPeriods.OrderByDescending(p => p.Year).FirstOrDefaultAsync(ct);

    public Task<NineBoxAssessment?> FindNineBoxAssessmentAsync(Guid talentId, Guid periodId, CancellationToken ct) =>
        db.NineBoxAssessments.FirstOrDefaultAsync(
            a => a.TalentId == talentId && a.EvaluationPeriodId == periodId, ct);

    public Task<NineBoxCellDefinition?> FindNineBoxCellAsync(PerformanceLevel perf, PotentialLevel pot, CancellationToken ct) =>
        db.NineBoxCellDefinitions.FirstOrDefaultAsync(
            c => c.PerformanceLevel == perf && c.PotentialLevel == pot, ct);

    public Task<CareerPlan?> FindCareerPlanAsync(Guid talentId, CancellationToken ct) =>
        db.CareerPlans.OrderByDescending(c => c.PeriodYear)
            .FirstOrDefaultAsync(c => c.UserId == talentId, ct);

    public async Task<IReadOnlyList<UserSkill>> GetTopSkillsAsync(Guid talentId, int count, CancellationToken ct) =>
        await db.UserSkills
            .Include(s => s.SkillItem)
                .ThenInclude(i => i!.Category)
            .Where(s => s.UserId == talentId && s.Status == UserSkillStatus.Approved)
            .OrderByDescending(s => s.Level)
            .Take(count)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<OneOnOneMeeting>> GetRecentMeetingsAsync(Guid talentId, int count, CancellationToken ct) =>
        await db.OneOnOneMeetings
            .Where(m => m.TalentId == talentId && !m.IsDeleted)
            .OrderByDescending(m => m.ConductedAt)
            .Take(count)
            .ToListAsync(ct);

    public Task<TalentManagerComment?> FindManagerCommentAsync(Guid talentId, Guid periodId, CancellationToken ct) =>
        db.TalentManagerComments.FirstOrDefaultAsync(
            c => c.TalentId == talentId && c.PeriodId == periodId, ct);

    public Task<KeyPerson?> FindActiveKeyPersonAsync(Guid talentId, CancellationToken ct) =>
        db.KeyPersons.FirstOrDefaultAsync(k => k.TalentId == talentId && k.IsActive, ct);

    public async Task AddManagerCommentAsync(TalentManagerComment comment, CancellationToken ct) =>
        await db.TalentManagerComments.AddAsync(comment, ct);

    public async Task AddKeyPersonAsync(KeyPerson keyPerson, CancellationToken ct) =>
        await db.KeyPersons.AddAsync(keyPerson, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
