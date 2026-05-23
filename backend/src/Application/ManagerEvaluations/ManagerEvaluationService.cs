using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Application.ManagerEvaluations;

public interface IManagerEvaluationService
{
    Task<Result<ManagerEvaluationDto>> GetAsync(Guid managerId, Guid selfEvaluationId, CancellationToken ct = default);
    Task<Result<ManagerEvaluationDto>> SaveDraftAsync(Guid managerId, Guid selfEvaluationId, SaveManagerDraftRequest req, CancellationToken ct = default);
    Task<Result<ManagerEvaluationDto>> ConfirmAsync(Guid managerId, Guid selfEvaluationId, CancellationToken ct = default);
}

public interface IManagerEvaluationRepository
{
    Task<SelfEvaluation?> FindSelfEvaluationAsync(Guid selfEvaluationId, CancellationToken ct);
    Task<ManagerEvaluation?> FindAsync(Guid selfEvaluationId, CancellationToken ct);
    Task AddAsync(ManagerEvaluation evaluation, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class ManagerEvaluationService(IManagerEvaluationRepository repo) : IManagerEvaluationService
{
    public async Task<Result<ManagerEvaluationDto>> GetAsync(Guid managerId, Guid selfEvaluationId, CancellationToken ct = default)
    {
        var selfEvaluation = await repo.FindSelfEvaluationAsync(selfEvaluationId, ct);
        if (selfEvaluation is null)
            return Result<ManagerEvaluationDto>.NotFound("自己評価が見つかりません。");

        var evaluation = await repo.FindAsync(selfEvaluationId, ct);
        if (evaluation is null)
            return Result<ManagerEvaluationDto>.NotFound("上長評価が見つかりません。");

        if (evaluation.ManagerId != managerId)
            return Result<ManagerEvaluationDto>.Forbidden("他の上長の評価は参照できません。");

        return Result<ManagerEvaluationDto>.Ok(ToDto(evaluation));
    }

    public async Task<Result<ManagerEvaluationDto>> SaveDraftAsync(Guid managerId, Guid selfEvaluationId, SaveManagerDraftRequest req, CancellationToken ct = default)
    {
        var selfEvaluation = await repo.FindSelfEvaluationAsync(selfEvaluationId, ct);
        if (selfEvaluation is null)
            return Result<ManagerEvaluationDto>.NotFound("自己評価が見つかりません。");

        if (!selfEvaluation.IsSubmitted)
            return Result<ManagerEvaluationDto>.Forbidden("タレントが自己評価を提出していません。");

        var evaluation = await repo.FindAsync(selfEvaluationId, ct);
        if (evaluation is null)
        {
            evaluation = ManagerEvaluation.Create(selfEvaluationId, managerId);
            await repo.AddAsync(evaluation, ct);
        }
        else if (evaluation.ManagerId != managerId)
        {
            return Result<ManagerEvaluationDto>.Forbidden("他の上長の評価は編集できません。");
        }

        if (evaluation.IsConfirmed)
            return Result<ManagerEvaluationDto>.Conflict("確定済みの上長評価は編集できません。");

        evaluation.UpdateDraft(req.OverallScore, req.FeedbackComment);
        await repo.SaveChangesAsync(ct);

        return Result<ManagerEvaluationDto>.Ok(ToDto(evaluation));
    }

    public async Task<Result<ManagerEvaluationDto>> ConfirmAsync(Guid managerId, Guid selfEvaluationId, CancellationToken ct = default)
    {
        var selfEvaluation = await repo.FindSelfEvaluationAsync(selfEvaluationId, ct);
        if (selfEvaluation is null)
            return Result<ManagerEvaluationDto>.NotFound("自己評価が見つかりません。");

        var evaluation = await repo.FindAsync(selfEvaluationId, ct);
        if (evaluation is null)
            return Result<ManagerEvaluationDto>.NotFound("上長評価が見つかりません。下書き保存を先に行ってください。");

        if (evaluation.ManagerId != managerId)
            return Result<ManagerEvaluationDto>.Forbidden("他の上長の評価は確定できません。");

        if (evaluation.IsConfirmed)
            return Result<ManagerEvaluationDto>.Conflict("既に確定済みです。");

        if (evaluation.OverallScore == 0)
            return Result<ManagerEvaluationDto>.BadRequest("スコアを入力してから確定してください。");

        evaluation.Confirm();
        await repo.SaveChangesAsync(ct);

        return Result<ManagerEvaluationDto>.Ok(ToDto(evaluation));
    }

    private static ManagerEvaluationDto ToDto(ManagerEvaluation e) => new(
        e.Id, e.SelfEvaluationId, e.ManagerId,
        e.OverallScore, e.FeedbackComment,
        e.IsConfirmed, e.ConfirmedAt,
        e.CreatedAt, e.UpdatedAt
    );
}
