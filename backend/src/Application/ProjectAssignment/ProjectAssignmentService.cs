using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.ProjectAssignment;

public interface IProjectAssignmentService
{
    Task<Result<ProjectAssignmentListResponse>> GetListAsync(Guid requesterId, Guid talentId, CancellationToken ct = default);
    Task<Result<ProjectAssignmentCreatedResponse>> CreateAsync(Guid requesterId, CreateProjectAssignmentRequest req, CancellationToken ct = default);
    Task<Result<ProjectAssignmentCreatedResponse>> UpdateAsync(Guid requesterId, Guid assignmentId, UpdateProjectAssignmentRequest req, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(Guid requesterId, Guid assignmentId, CancellationToken ct = default);
    Task<Result<ProjectAssignmentCreatedResponse>> SaveManagerCommentAsync(Guid requesterId, Guid assignmentId, SaveManagerCommentRequest req, CancellationToken ct = default);
}

public interface IProjectAssignmentRepository
{
    Task<User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<ProjectAssignmentDto>> GetListByTalentIdAsync(Guid talentId, CancellationToken ct);
    Task<Domain.Entities.ProjectAssignment?> FindByIdWithSkillsAsync(Guid id, CancellationToken ct);
    Task AddAsync(Domain.Entities.ProjectAssignment assignment, CancellationToken ct);
    Task<bool> SkillItemsExistAsync(IEnumerable<Guid> skillItemIds, CancellationToken ct);
    void Delete(Domain.Entities.ProjectAssignment assignment);
    Task SaveChangesAsync(CancellationToken ct);
}

public class ProjectAssignmentService(IProjectAssignmentRepository repo) : IProjectAssignmentService
{
    private static readonly HashSet<UserRole> ViewerRoles =
        [UserRole.Manager, UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<ProjectAssignmentListResponse>> GetListAsync(
        Guid requesterId, Guid talentId, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null)
            return Result<ProjectAssignmentListResponse>.Forbidden("ユーザーが見つかりません。");

        if (requesterId != talentId && !ViewerRoles.Contains(requester.Role))
            return Result<ProjectAssignmentListResponse>.Forbidden("プロジェクト履歴を参照する権限がありません。");

        var assignments = await repo.GetListByTalentIdAsync(talentId, ct);
        return Result<ProjectAssignmentListResponse>.Ok(new ProjectAssignmentListResponse(assignments));
    }

    public async Task<Result<ProjectAssignmentCreatedResponse>> CreateAsync(
        Guid requesterId, CreateProjectAssignmentRequest req, CancellationToken ct = default)
    {
        if (req.EndDate.HasValue && req.EndDate.Value < req.StartDate)
            return Result<ProjectAssignmentCreatedResponse>.BadRequest("終了日は開始日以降を指定してください。");

        if (req.StartDate > DateOnly.FromDateTime(DateTime.UtcNow))
            return Result<ProjectAssignmentCreatedResponse>.BadRequest("開始日に未来の日付は指定できません。");

        if (req.SkillItemIds.Count > 0 && !await repo.SkillItemsExistAsync(req.SkillItemIds, ct))
            return Result<ProjectAssignmentCreatedResponse>.BadRequest("存在しないスキルアイテムが含まれています。");

        var assignment = Domain.Entities.ProjectAssignment.Create(
            requesterId, req.ProjectName, req.Role, req.StartDate, req.EndDate, req.Description);

        foreach (var skillId in req.SkillItemIds.Distinct())
            assignment.Skills.Add(ProjectAssignmentSkill.Create(assignment.Id, skillId));

        await repo.AddAsync(assignment, ct);
        await repo.SaveChangesAsync(ct);

        return Result<ProjectAssignmentCreatedResponse>.Created(new ProjectAssignmentCreatedResponse(assignment.Id));
    }

    public async Task<Result<ProjectAssignmentCreatedResponse>> UpdateAsync(
        Guid requesterId, Guid assignmentId, UpdateProjectAssignmentRequest req, CancellationToken ct = default)
    {
        var assignment = await repo.FindByIdWithSkillsAsync(assignmentId, ct);
        if (assignment is null)
            return Result<ProjectAssignmentCreatedResponse>.NotFound("指定されたプロジェクト履歴が見つかりません。");

        if (assignment.TalentId != requesterId)
            return Result<ProjectAssignmentCreatedResponse>.Forbidden("本人のみ更新できます。");

        if (req.EndDate.HasValue && req.EndDate.Value < req.StartDate)
            return Result<ProjectAssignmentCreatedResponse>.BadRequest("終了日は開始日以降を指定してください。");

        if (req.StartDate > DateOnly.FromDateTime(DateTime.UtcNow))
            return Result<ProjectAssignmentCreatedResponse>.BadRequest("開始日に未来の日付は指定できません。");

        if (req.SkillItemIds.Count > 0 && !await repo.SkillItemsExistAsync(req.SkillItemIds, ct))
            return Result<ProjectAssignmentCreatedResponse>.BadRequest("存在しないスキルアイテムが含まれています。");

        assignment.Update(req.ProjectName, req.Role, req.StartDate, req.EndDate, req.Description);

        // スキルの差し替え
        assignment.Skills.Clear();
        foreach (var skillId in req.SkillItemIds.Distinct())
            assignment.Skills.Add(ProjectAssignmentSkill.Create(assignment.Id, skillId));

        await repo.SaveChangesAsync(ct);
        return Result<ProjectAssignmentCreatedResponse>.Ok(new ProjectAssignmentCreatedResponse(assignment.Id));
    }

    public async Task<Result<bool>> DeleteAsync(Guid requesterId, Guid assignmentId, CancellationToken ct = default)
    {
        var assignment = await repo.FindByIdWithSkillsAsync(assignmentId, ct);
        if (assignment is null)
            return Result<bool>.NotFound("指定されたプロジェクト履歴が見つかりません。");

        if (assignment.TalentId != requesterId)
            return Result<bool>.Forbidden("本人のみ削除できます。");

        repo.Delete(assignment);
        await repo.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<ProjectAssignmentCreatedResponse>> SaveManagerCommentAsync(
        Guid requesterId, Guid assignmentId, SaveManagerCommentRequest req, CancellationToken ct = default)
    {
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null || requester.Role != UserRole.Manager)
            return Result<ProjectAssignmentCreatedResponse>.Forbidden("上長のみコメントを追加できます。");

        var assignment = await repo.FindByIdWithSkillsAsync(assignmentId, ct);
        if (assignment is null)
            return Result<ProjectAssignmentCreatedResponse>.NotFound("指定されたプロジェクト履歴が見つかりません。");

        assignment.SetManagerComment(req.Comment, requesterId);
        await repo.SaveChangesAsync(ct);
        return Result<ProjectAssignmentCreatedResponse>.Ok(new ProjectAssignmentCreatedResponse(assignment.Id));
    }
}

// Response / DTOs
public record ProjectAssignmentListResponse(IReadOnlyList<ProjectAssignmentDto> Assignments);

public record ProjectAssignmentDto(
    Guid Id,
    string ProjectName,
    string Role,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    string? Description,
    string? ManagerComment,
    IReadOnlyList<ProjectAssignmentSkillDto> Skills,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record ProjectAssignmentSkillDto(Guid SkillItemId, string SkillName, string CategoryName);

public record ProjectAssignmentCreatedResponse(Guid Id);

// Requests
public record CreateProjectAssignmentRequest(
    string ProjectName,
    string Role,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Description,
    List<Guid> SkillItemIds
);

public record UpdateProjectAssignmentRequest(
    string ProjectName,
    string Role,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Description,
    List<Guid> SkillItemIds
);

public record SaveManagerCommentRequest(string Comment);
