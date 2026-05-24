using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.FeedbackMeeting;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class FeedbackMeetingRepository(AppDbContext db) : IFeedbackMeetingRepository
{
    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task<SelfEvaluation?> FindSelfEvaluationAsync(Guid selfEvaluationId, CancellationToken ct) =>
        db.SelfEvaluations.FirstOrDefaultAsync(s => s.Id == selfEvaluationId, ct);

    public Task<EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct) =>
        db.EvaluationPeriods.FirstOrDefaultAsync(p => p.Id == periodId, ct);

    public Task<FeedbackMeeting?> FindBySelfEvaluationAsync(Guid selfEvaluationId, CancellationToken ct) =>
        db.FeedbackMeetings.FirstOrDefaultAsync(m => m.SelfEvaluationId == selfEvaluationId, ct);

    public Task<FeedbackMeeting?> FindByTalentAndPeriodAsync(Guid talentId, Guid periodId, bool disclosedOnly, CancellationToken ct) =>
        db.FeedbackMeetings
            .Where(m => m.TalentId == talentId && m.SelfEvaluation!.EvaluationPeriodId == periodId)
            .Where(m => !disclosedOnly || m.IsDisclosed)
            .FirstOrDefaultAsync(ct);

    public Task<FeedbackMeeting?> FindAsync(Guid id, CancellationToken ct) =>
        db.FeedbackMeetings.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task AddAsync(FeedbackMeeting meeting, CancellationToken ct) =>
        await db.FeedbackMeetings.AddAsync(meeting, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
