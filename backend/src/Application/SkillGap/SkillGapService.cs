using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.SkillGap;

public interface ISkillGapService
{
    Task<Result<SkillGapResponse>> GetGapAsync(Guid requesterId, Guid talentId, Guid periodId, int threshold, CancellationToken ct = default);
}

public interface ISkillGapRepository
{
    Task<UserRole?> GetUserRoleAsync(Guid userId, CancellationToken ct);
    Task<bool> IsManagedByAsync(Guid managerId, Guid talentId, CancellationToken ct);
    Task<IReadOnlyList<SkillSelfLevelDto>> GetApprovedSkillLevelsAsync(Guid talentId, CancellationToken ct);
    Task<IReadOnlyList<CompetencyGapRawDto>> GetCompetencyGapDataAsync(Guid talentId, Guid periodId, CancellationToken ct);
}

public record SkillSelfLevelDto(Guid SkillItemId, string SkillItemName, int Level);

public record CompetencyGapRawDto(
    Guid CompetencyId,
    string CompetencyName,
    string Category,
    CompetencyEvaluatorType EvaluatorType,
    int Level);

public class SkillGapService(ISkillGapRepository repo) : ISkillGapService
{
    public async Task<Result<SkillGapResponse>> GetGapAsync(
        Guid requesterId,
        Guid talentId,
        Guid periodId,
        int threshold,
        CancellationToken ct = default)
    {
        var role = await repo.GetUserRoleAsync(requesterId, ct);
        if (role is null)
            return Result<SkillGapResponse>.Forbidden("ユーザーが見つかりません。");

        if (role == UserRole.Talent && requesterId != talentId)
            return Result<SkillGapResponse>.Forbidden("自分のギャップのみ閲覧できます。");

        if (role == UserRole.Manager && !await repo.IsManagedByAsync(requesterId, talentId, ct))
            return Result<SkillGapResponse>.Forbidden("配下のタレントのみ閲覧できます。");

        var skillLevels = await repo.GetApprovedSkillLevelsAsync(talentId, ct);
        var competencyData = await repo.GetCompetencyGapDataAsync(talentId, periodId, ct);

        var skillItems = skillLevels.Select(s =>
        {
            // Manager skill level is not separately tracked in current data model
            return new SkillGapItemDto(s.SkillItemId, s.SkillItemName, s.Level, null, null, false);
        }).ToList();

        var competencyItems = competencyData
            .GroupBy(e => e.CompetencyId)
            .Select(g =>
            {
                var self = g.FirstOrDefault(e => e.EvaluatorType == CompetencyEvaluatorType.Self);
                var manager = g.FirstOrDefault(e => e.EvaluatorType == CompetencyEvaluatorType.Manager);
                var first = g.First();
                int? gap = self is not null && manager is not null ? manager.Level - self.Level : null;
                bool requiresAttention = gap.HasValue && Math.Abs(gap.Value) >= threshold;
                return new CompetencyGapItemDto(
                    first.CompetencyId,
                    first.CompetencyName,
                    first.Category,
                    self?.Level ?? 0,
                    manager?.Level,
                    gap,
                    requiresAttention);
            })
            .Where(c => c.SelfLevel > 0)
            .ToList();

        return Result<SkillGapResponse>.Ok(new SkillGapResponse(talentId, periodId, threshold, skillItems, competencyItems));
    }
}
