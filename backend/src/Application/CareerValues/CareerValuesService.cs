using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Enums;
using DomainEntities = TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Application.CareerValues;

// ── DTOs ────────────────────────────────────────────────────────────────────

public record CareerValuesDto(
    Guid? Id,
    Guid TalentId,
    Guid PeriodId,
    string? ImportantThings,
    string? MotivationSituations,
    string? AvoidSituations,
    DateTimeOffset? UpdatedAt);

public record SaveCareerValuesRequest(
    Guid TalentId,
    Guid PeriodId,
    string? ImportantThings,
    string? MotivationSituations,
    string? AvoidSituations);

// ── Repository Interface ─────────────────────────────────────────────────────

public interface ICareerValuesRepository
{
    Task<DomainEntities.User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<DomainEntities.EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct);
    Task<DomainEntities.CareerValues?> FindAsync(Guid talentId, Guid periodId, CancellationToken ct);
    Task AddAsync(DomainEntities.CareerValues values, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

// ── Service Interface ────────────────────────────────────────────────────────

public interface ICareerValuesService
{
    Task<Result<CareerValuesDto>> GetValuesAsync(Guid requesterId, Guid talentId, Guid periodId, CancellationToken ct = default);
    Task<Result<CareerValuesDto>> SaveValuesAsync(Guid requesterId, SaveCareerValuesRequest req, CancellationToken ct = default);
}

// ── Service Implementation ───────────────────────────────────────────────────

public class CareerValuesService(ICareerValuesRepository repo) : ICareerValuesService
{
    private static readonly HashSet<UserRole> SeniorRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<CareerValuesDto>> GetValuesAsync(Guid requesterId, Guid talentId, Guid periodId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<CareerValuesDto>.Forbidden("権限がありません。");

        if (!CanView(requester, talentId))
            return Result<CareerValuesDto>.Forbidden("このタレントの価値観情報を閲覧する権限がありません。");

        var period = await repo.FindPeriodAsync(periodId, ct);
        if (period is null)
            return Result<CareerValuesDto>.NotFound("考課期間が見つかりません。");

        var talent = await repo.FindUserAsync(talentId, ct);
        if (talent is null)
            return Result<CareerValuesDto>.NotFound("タレントが見つかりません。");

        var values = await repo.FindAsync(talentId, periodId, ct);
        return Result<CareerValuesDto>.Ok(ToDto(talentId, periodId, values));
    }

    public async Task<Result<CareerValuesDto>> SaveValuesAsync(Guid requesterId, SaveCareerValuesRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<CareerValuesDto>.Forbidden("権限がありません。");

        if (requesterId != req.TalentId)
            return Result<CareerValuesDto>.Forbidden("自分の価値観情報のみ保存できます。");

        var period = await repo.FindPeriodAsync(req.PeriodId, ct);
        if (period is null)
            return Result<CareerValuesDto>.NotFound("考課期間が見つかりません。");

        var talent = await repo.FindUserAsync(req.TalentId, ct);
        if (talent is null)
            return Result<CareerValuesDto>.NotFound("タレントが見つかりません。");

        var existing = await repo.FindAsync(req.TalentId, req.PeriodId, ct);
        if (existing is not null)
        {
            existing.Update(req.ImportantThings, req.MotivationSituations, req.AvoidSituations);
        }
        else
        {
            var newValues = DomainEntities.CareerValues.Create(
                req.TalentId, req.PeriodId,
                req.ImportantThings, req.MotivationSituations, req.AvoidSituations);
            await repo.AddAsync(newValues, ct);
        }

        await repo.SaveChangesAsync(ct);

        var saved = await repo.FindAsync(req.TalentId, req.PeriodId, ct);
        return Result<CareerValuesDto>.Ok(ToDto(req.TalentId, req.PeriodId, saved));
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static bool CanView(DomainEntities.User requester, Guid talentId)
    {
        if (requester.Id == talentId) return true;
        if (SeniorRoles.Contains(requester.Role)) return true;
        if (requester.Role == UserRole.Manager) return true;
        return false;
    }

    private static CareerValuesDto ToDto(Guid talentId, Guid periodId, DomainEntities.CareerValues? values) =>
        new(values?.Id, talentId, periodId,
            values?.ImportantThings, values?.MotivationSituations, values?.AvoidSituations,
            values?.UpdatedAt);
}
