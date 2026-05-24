using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Application.SkillLevelDefinition;

public interface ISkillLevelDefinitionService
{
    Task<Result<SkillLevelDefinitionsResponse>> GetDefinitionsAsync(Guid skillItemId, CancellationToken ct = default);
    Task<Result<SkillLevelDefinitionsResponse>> SaveDefinitionsAsync(Guid requesterId, Guid skillItemId, SaveSkillLevelDefinitionsRequest req, CancellationToken ct = default);
}

public interface ISkillLevelDefinitionRepository
{
    Task<bool> IsAdminAsync(Guid userId, CancellationToken ct);
    Task<bool> SkillItemExistsAsync(Guid skillItemId, CancellationToken ct);
    Task<IReadOnlyList<Domain.Entities.SkillLevelDefinition>> GetBySkillItemAsync(Guid skillItemId, CancellationToken ct);
    Task UpsertRangeAsync(IEnumerable<Domain.Entities.SkillLevelDefinition> definitions, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class SkillLevelDefinitionService(ISkillLevelDefinitionRepository repo) : ISkillLevelDefinitionService
{
    private static readonly IReadOnlyDictionary<int, string> FallbackDescriptions = new Dictionary<int, string>
    {
        { 1, "概念的理解。学習中または経験ゼロ" },
        { 2, "基礎的な作業を上位者のサポートで実施可能" },
        { 3, "単独で標準的な業務を遂行可能" },
        { 4, "応用的な業務を主導し、後輩・同僚への技術指導が可能" },
        { 5, "専門家として高度な業務を自律的に遂行可能" },
        { 6, "組織横断的なプロジェクトをリードし、技術戦略に貢献" },
        { 7, "業界・社内でのエキスパートとして標準や戦略を定義" }
    };

    public async Task<Result<SkillLevelDefinitionsResponse>> GetDefinitionsAsync(Guid skillItemId, CancellationToken ct = default)
    {
        if (!await repo.SkillItemExistsAsync(skillItemId, ct))
            return Result<SkillLevelDefinitionsResponse>.NotFound("スキル種別が見つかりません。");

        var stored = await repo.GetBySkillItemAsync(skillItemId, ct);
        var storedByLevel = stored.ToDictionary(d => d.Level);

        var defs = Enumerable.Range(1, 7).Select(level =>
        {
            if (storedByLevel.TryGetValue(level, out var d))
                return new SkillLevelDefinitionDto(level, d.Description, d.Examples, false);
            return new SkillLevelDefinitionDto(level, FallbackDescriptions[level], null, true);
        }).ToList();

        return Result<SkillLevelDefinitionsResponse>.Ok(new SkillLevelDefinitionsResponse(skillItemId, defs));
    }

    public async Task<Result<SkillLevelDefinitionsResponse>> SaveDefinitionsAsync(Guid requesterId, Guid skillItemId, SaveSkillLevelDefinitionsRequest req, CancellationToken ct = default)
    {
        if (!await repo.IsAdminAsync(requesterId, ct))
            return Result<SkillLevelDefinitionsResponse>.Forbidden("管理者のみ操作できます。");

        if (!await repo.SkillItemExistsAsync(skillItemId, ct))
            return Result<SkillLevelDefinitionsResponse>.NotFound("スキル種別が見つかりません。");

        if (req.LevelDefinitions.Any(d => d.Level < 1 || d.Level > 7))
            return Result<SkillLevelDefinitionsResponse>.BadRequest("レベルは1〜7の範囲で指定してください。");

        if (req.LevelDefinitions.Any(d => string.IsNullOrWhiteSpace(d.Description)))
            return Result<SkillLevelDefinitionsResponse>.BadRequest("説明は必須です。");

        var stored = await repo.GetBySkillItemAsync(skillItemId, ct);
        var storedByLevel = stored.ToDictionary(d => d.Level);

        var toUpsert = new List<Domain.Entities.SkillLevelDefinition>();
        foreach (var item in req.LevelDefinitions)
        {
            if (storedByLevel.TryGetValue(item.Level, out var existing))
            {
                existing.Update(item.Description, item.Examples);
                toUpsert.Add(existing);
            }
            else
            {
                toUpsert.Add(Domain.Entities.SkillLevelDefinition.Create(skillItemId, item.Level, item.Description, item.Examples));
            }
        }

        await repo.UpsertRangeAsync(toUpsert, ct);
        await repo.SaveChangesAsync(ct);

        return await GetDefinitionsAsync(skillItemId, ct);
    }
}
