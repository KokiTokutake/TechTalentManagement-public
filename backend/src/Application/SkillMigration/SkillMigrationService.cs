using System.Text.Json;
using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.SkillMigration;

public interface ISkillMigrationService
{
    Task<Result<MigrationPreviewResponse>> GetPreviewAsync(Guid requesterId, CancellationToken ct = default);
    Task<Result<MigrationExecuteResponse>> ExecuteAsync(Guid requesterId, CancellationToken ct = default);
}

public interface ISkillMigrationRepository
{
    Task<UserRole?> GetUserRoleAsync(Guid userId, CancellationToken ct);
    Task<bool> MigrationLogExistsAsync(CancellationToken ct);
    Task<IReadOnlyList<LegacySkillLevelDto>> GetLegacySkillsAsync(CancellationToken ct);
    Task UpdateSkillLevelAsync(Guid userSkillId, int newLevel, CancellationToken ct);
    Task AddMigrationLogAsync(SkillLevelMigrationLog log, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public record LegacySkillLevelDto(Guid UserSkillId, string TalentName, string SkillName, int CurrentLevel);

public class SkillMigrationService(ISkillMigrationRepository repo) : ISkillMigrationService
{
    private static readonly Dictionary<int, int> ConversionRule = new()
    {
        { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 5 }, { 5, 7 }
    };

    public async Task<Result<MigrationPreviewResponse>> GetPreviewAsync(Guid requesterId, CancellationToken ct = default)
    {
        var role = await repo.GetUserRoleAsync(requesterId, ct);
        if (role is null || role != UserRole.Admin)
            return Result<MigrationPreviewResponse>.Forbidden("管理者のみ操作できます。");

        var legacySkills = await repo.GetLegacySkillsAsync(ct);
        var eligible = legacySkills.Where(s => ConversionRule.ContainsKey(s.CurrentLevel)).ToList();

        var preview = eligible
            .Take(100)
            .Select(s => new MigrationPreviewItem(s.TalentName, s.SkillName, s.CurrentLevel, ConversionRule[s.CurrentLevel]))
            .ToList();

        return Result<MigrationPreviewResponse>.Ok(new MigrationPreviewResponse(
            new Dictionary<int, int>(ConversionRule),
            eligible.Count,
            preview));
    }

    public async Task<Result<MigrationExecuteResponse>> ExecuteAsync(Guid requesterId, CancellationToken ct = default)
    {
        var role = await repo.GetUserRoleAsync(requesterId, ct);
        if (role is null || role != UserRole.Admin)
            return Result<MigrationExecuteResponse>.Forbidden("管理者のみ操作できます。");

        if (await repo.MigrationLogExistsAsync(ct))
            return Result<MigrationExecuteResponse>.Conflict("移行ログが既に存在します。二重実行を防止するために実行を中止しました。");

        var legacySkills = await repo.GetLegacySkillsAsync(ct);
        var eligible = legacySkills.Where(s => ConversionRule.ContainsKey(s.CurrentLevel)).ToList();

        foreach (var skill in eligible)
            await repo.UpdateSkillLevelAsync(skill.UserSkillId, ConversionRule[skill.CurrentLevel], ct);

        var log = SkillLevelMigrationLog.Create(requesterId, eligible.Count, JsonSerializer.Serialize(ConversionRule));
        await repo.AddMigrationLogAsync(log, ct);
        await repo.SaveChangesAsync(ct);

        return Result<MigrationExecuteResponse>.Ok(new MigrationExecuteResponse(true, eligible.Count, log.Id, log.ExecutedAt));
    }
}
