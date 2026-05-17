using TeckTalentManagement.Application.Common;
using TeckTalentManagement.Domain.Entities;
using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Application.UserSkills;

public interface IUserSkillService
{
    Task<Result<IReadOnlyList<UserSkillDto>>> GetUserSkillsAsync(Guid userId, CancellationToken ct = default);
    Task<Result<UserSkillDto>> AddSkillAsync(Guid requesterId, Guid targetUserId, AddUserSkillRequest req, CancellationToken ct = default);
    Task<Result<UserSkillDto>> UpdateSkillAsync(Guid requesterId, Guid userSkillId, UpdateUserSkillRequest req, CancellationToken ct = default);
    Task<Result<bool>> DeleteSkillAsync(Guid requesterId, Guid userSkillId, CancellationToken ct = default);
    Task<Result<UserSkillDto>> ApproveSkillAsync(Guid requesterId, Guid userSkillId, CancellationToken ct = default);
    Task<Result<UserSkillDto>> RejectSkillAsync(Guid requesterId, Guid userSkillId, string reason, CancellationToken ct = default);
}

public interface IUserSkillRepository
{
    Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task<UserSkill?> FindAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid userId, Guid skillItemId, CancellationToken ct);
    Task<SkillItem?> FindSkillItemAsync(Guid skillItemId, CancellationToken ct);
    Task<bool> IsManagerOfAsync(Guid managerId, Guid userId, CancellationToken ct);
    Task AddAsync(UserSkill userSkill, CancellationToken ct);
    Task DeleteAsync(UserSkill userSkill, CancellationToken ct);
    Task AddHistoryAsync(UserSkillHistory history, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class UserSkillService(IUserSkillRepository repo) : IUserSkillService
{
    public async Task<Result<IReadOnlyList<UserSkillDto>>> GetUserSkillsAsync(Guid userId, CancellationToken ct = default)
    {
        var skills = await repo.GetByUserIdAsync(userId, ct);
        return Result<IReadOnlyList<UserSkillDto>>.Ok(skills.Select(ToDto).ToList());
    }

    public async Task<Result<UserSkillDto>> AddSkillAsync(Guid requesterId, Guid targetUserId, AddUserSkillRequest req, CancellationToken ct = default)
    {
        if (requesterId != targetUserId)
            return Result<UserSkillDto>.Forbidden("自分のスキルのみ追加できます。");

        if (req.Level is < 1 or > 5)
            return Result<UserSkillDto>.BadRequest("レベルは1〜5で指定してください。");

        var skillItem = await repo.FindSkillItemAsync(req.SkillItemId, ct);
        if (skillItem is null)
            return Result<UserSkillDto>.NotFound("スキル種別が見つかりません。");

        if (await repo.ExistsAsync(targetUserId, req.SkillItemId, ct))
            return Result<UserSkillDto>.Conflict("既に登録されています。");

        var userSkill = UserSkill.Create(targetUserId, req.SkillItemId, req.Level, req.Comment);
        await repo.AddAsync(userSkill, ct);

        var history = UserSkillHistory.Create(userSkill.Id, userSkill.Level, userSkill.Status, requesterId);
        await repo.AddHistoryAsync(history, ct);

        await repo.SaveChangesAsync(ct);

        userSkill = (await repo.FindAsync(userSkill.Id, ct))!;
        return Result<UserSkillDto>.Ok(ToDto(userSkill));
    }

    public async Task<Result<UserSkillDto>> UpdateSkillAsync(Guid requesterId, Guid userSkillId, UpdateUserSkillRequest req, CancellationToken ct = default)
    {
        if (req.Level is < 1 or > 5)
            return Result<UserSkillDto>.BadRequest("レベルは1〜5で指定してください。");

        var userSkill = await repo.FindAsync(userSkillId, ct);
        if (userSkill is null)
            return Result<UserSkillDto>.NotFound("スキルが見つかりません。");

        if (userSkill.UserId != requesterId)
            return Result<UserSkillDto>.Forbidden("自分のスキルのみ編集できます。");

        userSkill.Update(req.Level, req.Comment);

        var history = UserSkillHistory.Create(userSkill.Id, userSkill.Level, userSkill.Status, requesterId);
        await repo.AddHistoryAsync(history, ct);

        await repo.SaveChangesAsync(ct);

        return Result<UserSkillDto>.Ok(ToDto(userSkill));
    }

    public async Task<Result<bool>> DeleteSkillAsync(Guid requesterId, Guid userSkillId, CancellationToken ct = default)
    {
        var userSkill = await repo.FindAsync(userSkillId, ct);
        if (userSkill is null)
            return Result<bool>.NotFound("スキルが見つかりません。");

        if (userSkill.UserId != requesterId)
            return Result<bool>.Forbidden("自分のスキルのみ削除できます。");

        await repo.DeleteAsync(userSkill, ct);
        await repo.SaveChangesAsync(ct);

        return Result<bool>.Ok(true);
    }

    public async Task<Result<UserSkillDto>> ApproveSkillAsync(Guid requesterId, Guid userSkillId, CancellationToken ct = default)
    {
        var userSkill = await repo.FindAsync(userSkillId, ct);
        if (userSkill is null)
            return Result<UserSkillDto>.NotFound("スキルが見つかりません。");

        if (!await repo.IsManagerOfAsync(requesterId, userSkill.UserId, ct))
            return Result<UserSkillDto>.Forbidden("部下のスキルのみ承認できます。");

        if (userSkill.Status != UserSkillStatus.Pending)
            return Result<UserSkillDto>.BadRequest("申請中のスキルのみ承認できます。");

        userSkill.Approve(requesterId);

        var history = UserSkillHistory.Create(userSkill.Id, userSkill.Level, userSkill.Status, requesterId);
        await repo.AddHistoryAsync(history, ct);

        await repo.SaveChangesAsync(ct);

        return Result<UserSkillDto>.Ok(ToDto(userSkill));
    }

    public async Task<Result<UserSkillDto>> RejectSkillAsync(Guid requesterId, Guid userSkillId, string reason, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result<UserSkillDto>.BadRequest("差し戻し理由は必須です。");

        var userSkill = await repo.FindAsync(userSkillId, ct);
        if (userSkill is null)
            return Result<UserSkillDto>.NotFound("スキルが見つかりません。");

        if (!await repo.IsManagerOfAsync(requesterId, userSkill.UserId, ct))
            return Result<UserSkillDto>.Forbidden("部下のスキルのみ差し戻しできます。");

        if (userSkill.Status != UserSkillStatus.Pending)
            return Result<UserSkillDto>.BadRequest("申請中のスキルのみ差し戻しできます。");

        userSkill.Reject(reason);

        var history = UserSkillHistory.Create(userSkill.Id, userSkill.Level, userSkill.Status, requesterId);
        await repo.AddHistoryAsync(history, ct);

        await repo.SaveChangesAsync(ct);

        return Result<UserSkillDto>.Ok(ToDto(userSkill));
    }

    private static UserSkillDto ToDto(UserSkill s) => new(
        s.Id,
        s.UserId,
        s.SkillItemId,
        s.SkillItem?.Name ?? string.Empty,
        s.SkillItem?.CategoryId ?? Guid.Empty,
        s.SkillItem?.Category?.Name ?? string.Empty,
        s.Level,
        s.Comment,
        s.Status.ToString(),
        s.ApprovedBy,
        s.ApprovedAt,
        s.RejectedReason,
        s.CreatedAt,
        s.UpdatedAt
    );
}
