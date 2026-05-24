using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.CompetencyEvaluation;

public interface ICompetencyEvaluationService
{
    Task<Result<SaveCompetencyEvaluationsResponse>> SaveEvaluationsAsync(Guid requesterId, SaveCompetencyEvaluationsRequest req, CancellationToken ct = default);
    Task<Result<CompetencyEvaluationsResponse>> GetEvaluationsAsync(Guid requesterId, Guid talentId, Guid periodId, CancellationToken ct = default);
}

public interface ICompetencyEvaluationRepository
{
    Task<UserRole?> GetUserRoleAsync(Guid userId, CancellationToken ct);
    Task<bool> IsManagedByAsync(Guid managerId, Guid talentId, CancellationToken ct);
    Task<bool> PeriodExistsAsync(Guid periodId, CancellationToken ct);
    Task<bool> CompetencyExistsAsync(Guid competencyId, CancellationToken ct);
    Task<Domain.Entities.CompetencyEvaluation?> FindEvaluationAsync(Guid talentId, Guid evaluatorId, Guid competencyId, Guid periodId, CompetencyEvaluatorType evaluatorType, CancellationToken ct);
    Task AddEvaluationAsync(Domain.Entities.CompetencyEvaluation evaluation, CancellationToken ct);
    Task<IReadOnlyList<CompetencyEvaluationWithMaster>> GetEvaluationsWithMasterAsync(Guid talentId, Guid periodId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public record CompetencyEvaluationWithMaster(
    Guid CompetencyId,
    string CompetencyName,
    string Category,
    CompetencyEvaluatorType EvaluatorType,
    int Level,
    string? Comment,
    DateTimeOffset UpdatedAt);

public class CompetencyEvaluationService(ICompetencyEvaluationRepository repo) : ICompetencyEvaluationService
{
    public async Task<Result<SaveCompetencyEvaluationsResponse>> SaveEvaluationsAsync(
        Guid requesterId,
        SaveCompetencyEvaluationsRequest req,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<CompetencyEvaluatorType>(req.EvaluatorType, out var evaluatorType))
            return Result<SaveCompetencyEvaluationsResponse>.BadRequest($"無効な評価者タイプです: {req.EvaluatorType}");

        var role = await repo.GetUserRoleAsync(requesterId, ct);
        if (role is null)
            return Result<SaveCompetencyEvaluationsResponse>.Forbidden("ユーザーが見つかりません。");

        if (evaluatorType == CompetencyEvaluatorType.Self && requesterId != req.TalentId)
            return Result<SaveCompetencyEvaluationsResponse>.Forbidden("自己評価は本人のみ実行できます。");

        if (evaluatorType == CompetencyEvaluatorType.Manager)
        {
            if (role is not (UserRole.Manager or UserRole.Senior or UserRole.Executive or UserRole.Admin))
                return Result<SaveCompetencyEvaluationsResponse>.Forbidden("上長評価は管理職以上のみ実行できます。");

            if (role == UserRole.Manager && !await repo.IsManagedByAsync(requesterId, req.TalentId, ct))
                return Result<SaveCompetencyEvaluationsResponse>.Forbidden("配下のタレントのみ評価できます。");
        }

        if (!await repo.PeriodExistsAsync(req.PeriodId, ct))
            return Result<SaveCompetencyEvaluationsResponse>.NotFound("評価期間が見つかりません。");

        int savedCount = 0;
        foreach (var item in req.Evaluations)
        {
            if (item.Level < 1 || item.Level > 5)
                return Result<SaveCompetencyEvaluationsResponse>.BadRequest($"コンピテンシーレベルは1〜5の範囲で入力してください。");

            if (!await repo.CompetencyExistsAsync(item.CompetencyId, ct))
                return Result<SaveCompetencyEvaluationsResponse>.NotFound($"コンピテンシーが見つかりません: {item.CompetencyId}");

            var existing = await repo.FindEvaluationAsync(req.TalentId, requesterId, item.CompetencyId, req.PeriodId, evaluatorType, ct);
            if (existing is not null)
                existing.Update(item.Level, item.Comment);
            else
                await repo.AddEvaluationAsync(
                    Domain.Entities.CompetencyEvaluation.Create(req.TalentId, requesterId, item.CompetencyId, req.PeriodId, item.Level, item.Comment, evaluatorType),
                    ct);

            savedCount++;
        }

        await repo.SaveChangesAsync(ct);
        return Result<SaveCompetencyEvaluationsResponse>.Ok(new SaveCompetencyEvaluationsResponse(savedCount));
    }

    public async Task<Result<CompetencyEvaluationsResponse>> GetEvaluationsAsync(
        Guid requesterId,
        Guid talentId,
        Guid periodId,
        CancellationToken ct = default)
    {
        var role = await repo.GetUserRoleAsync(requesterId, ct);
        if (role is null)
            return Result<CompetencyEvaluationsResponse>.Forbidden("ユーザーが見つかりません。");

        if (role == UserRole.Talent && requesterId != talentId)
            return Result<CompetencyEvaluationsResponse>.Forbidden("自分の評価のみ閲覧できます。");

        if (role == UserRole.Manager && !await repo.IsManagedByAsync(requesterId, talentId, ct))
            return Result<CompetencyEvaluationsResponse>.Forbidden("配下のタレントのみ閲覧できます。");

        var rawEvals = await repo.GetEvaluationsWithMasterAsync(talentId, periodId, ct);

        var grouped = rawEvals
            .GroupBy(e => e.CompetencyId)
            .Select(g =>
            {
                var self = g.FirstOrDefault(e => e.EvaluatorType == CompetencyEvaluatorType.Self);
                var manager = g.FirstOrDefault(e => e.EvaluatorType == CompetencyEvaluatorType.Manager);
                var first = g.First();
                int? gap = self is not null && manager is not null ? manager.Level - self.Level : null;
                return new CompetencyEvaluationResultDto(
                    first.CompetencyId,
                    first.CompetencyName,
                    first.Category,
                    self is not null ? new EvaluationDetailDto(self.Level, self.Comment, self.UpdatedAt) : null,
                    manager is not null ? new EvaluationDetailDto(manager.Level, manager.Comment, manager.UpdatedAt) : null,
                    gap);
            })
            .ToList();

        return Result<CompetencyEvaluationsResponse>.Ok(new CompetencyEvaluationsResponse(talentId, periodId, grouped));
    }
}
