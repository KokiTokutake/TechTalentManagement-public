using TeckTalentManagement.Application.Common;
using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Application.CareerPlan;

public interface ICareerPlanService
{
    Task<Result<CareerPlanDto>> GetAsync(Guid requesterId, Guid userId, int? year, CancellationToken ct = default);
    Task<Result<IReadOnlyList<int>>> GetYearsAsync(Guid requesterId, Guid userId, CancellationToken ct = default);
    Task<Result<CareerPlanDto>> CreateAsync(Guid requesterId, Guid userId, SaveCareerPlanRequest req, CancellationToken ct = default);
    Task<Result<CareerPlanDto>> UpdateAsync(Guid requesterId, Guid careerPlanId, SaveCareerPlanRequest req, CancellationToken ct = default);
    Task<Result<IntegratedViewDto>> GetIntegratedViewAsync(Guid requesterId, Guid userId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CareerPlanCommentDto>>> GetCommentsAsync(Guid requesterId, Guid careerPlanId, CancellationToken ct = default);
    Task<Result<CareerPlanCommentDto>> AddCommentAsync(Guid requesterId, Guid careerPlanId, AddCommentRequest req, CancellationToken ct = default);
}

public interface ICareerPlanRepository
{
    Task<Domain.Entities.CareerPlan?> FindByUserAndYearAsync(Guid userId, int year, CancellationToken ct);
    Task<Domain.Entities.CareerPlan?> FindAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<int>> GetYearsAsync(Guid userId, CancellationToken ct);
    Task<Domain.Entities.User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<bool> IsManagerOfAsync(Guid managerId, Guid userId, CancellationToken ct);
    Task AddAsync(Domain.Entities.CareerPlan plan, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task<IReadOnlyList<Domain.Entities.UserSkill>> GetApprovedSkillsAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<Domain.Entities.Goal>> GetCurrentYearGoalsAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<Domain.Entities.CareerPlanComment>> GetCommentsAsync(Guid careerPlanId, CancellationToken ct);
    Task AddCommentAsync(Domain.Entities.CareerPlanComment comment, CancellationToken ct);
}

public class CareerPlanService(ICareerPlanRepository repo) : ICareerPlanService
{
    private static readonly HashSet<UserRole> ViewerRoles = [UserRole.Senior, UserRole.Executive, UserRole.Admin];

    public async Task<Result<CareerPlanDto>> GetAsync(Guid requesterId, Guid userId, int? year, CancellationToken ct = default)
    {
        if (!await CanReadAsync(requesterId, userId, ct))
            return Result<CareerPlanDto>.Forbidden("このキャリアプランを閲覧する権限がありません。");

        var targetYear = year ?? DateTime.UtcNow.Year;
        var plan = await repo.FindByUserAndYearAsync(userId, targetYear, ct);
        if (plan is null)
            return Result<CareerPlanDto>.NotFound($"{targetYear}年度のキャリアプランが存在しません。");

        return Result<CareerPlanDto>.Ok(ToDto(plan));
    }

    public async Task<Result<IReadOnlyList<int>>> GetYearsAsync(Guid requesterId, Guid userId, CancellationToken ct = default)
    {
        if (!await CanReadAsync(requesterId, userId, ct))
            return Result<IReadOnlyList<int>>.Forbidden("このキャリアプランを閲覧する権限がありません。");

        var years = await repo.GetYearsAsync(userId, ct);
        return Result<IReadOnlyList<int>>.Ok(years);
    }

    public async Task<Result<CareerPlanDto>> CreateAsync(Guid requesterId, Guid userId, SaveCareerPlanRequest req, CancellationToken ct = default)
    {
        if (requesterId != userId)
            return Result<CareerPlanDto>.Forbidden("自分のキャリアプランのみ作成できます。");

        if (!ValidateLength(req, out var fieldError))
            return Result<CareerPlanDto>.BadRequest(fieldError);

        var currentYear = DateTime.UtcNow.Year;
        var existing = await repo.FindByUserAndYearAsync(userId, currentYear, ct);
        if (existing is not null)
            return Result<CareerPlanDto>.Conflict($"{currentYear}年度のキャリアプランはすでに存在します。");

        var plan = Domain.Entities.CareerPlan.Create(userId, req.LongTermGoal, req.MediumTermGoal, req.CurrentFocus);
        await repo.AddAsync(plan, ct);
        await repo.SaveChangesAsync(ct);

        return Result<CareerPlanDto>.Ok(ToDto(plan));
    }

    public async Task<Result<IntegratedViewDto>> GetIntegratedViewAsync(Guid requesterId, Guid userId, CancellationToken ct = default)
    {
        if (!await CanReadAsync(requesterId, userId, ct))
            return Result<IntegratedViewDto>.Forbidden("このキャリアプランを閲覧する権限がありません。");

        var currentYear = DateTime.UtcNow.Year;
        var plan = await repo.FindByUserAndYearAsync(userId, currentYear, ct);
        var skills = await repo.GetApprovedSkillsAsync(userId, ct);
        var goals = await repo.GetCurrentYearGoalsAsync(userId, ct);

        var will = plan is not null ? ToDto(plan) : null;
        var can = skills.Select(s => new IntegratedViewSkillDto(
            s.Id, s.SkillItemId,
            s.SkillItem?.Name ?? string.Empty,
            s.SkillItem?.Category?.Name ?? string.Empty,
            s.Level
        )).ToList();
        var must = goals.Select(g => new IntegratedViewGoalDto(
            g.Id, g.Category.ToString(), g.Title, g.Description, g.DisplayOrder
        )).ToList();

        return Result<IntegratedViewDto>.Ok(new IntegratedViewDto(will, can, must));
    }

    public async Task<Result<CareerPlanDto>> UpdateAsync(Guid requesterId, Guid careerPlanId, SaveCareerPlanRequest req, CancellationToken ct = default)
    {
        var plan = await repo.FindAsync(careerPlanId, ct);
        if (plan is null)
            return Result<CareerPlanDto>.NotFound("キャリアプランが存在しません。");

        if (requesterId != plan.UserId)
            return Result<CareerPlanDto>.Forbidden("自分のキャリアプランのみ編集できます。");

        if (plan.PeriodYear != DateTime.UtcNow.Year)
            return Result<CareerPlanDto>.Forbidden("過去の期のキャリアプランは編集できません。");

        if (!ValidateLength(req, out var fieldError))
            return Result<CareerPlanDto>.BadRequest(fieldError);

        plan.Update(req.LongTermGoal, req.MediumTermGoal, req.CurrentFocus);
        await repo.SaveChangesAsync(ct);

        return Result<CareerPlanDto>.Ok(ToDto(plan));
    }

    private async Task<bool> CanReadAsync(Guid requesterId, Guid userId, CancellationToken ct)
    {
        if (requesterId == userId) return true;
        var requester = await repo.FindUserAsync(requesterId, ct);
        if (requester is null) return false;
        if (ViewerRoles.Contains(requester.Role)) return true;
        if (requester.Role == UserRole.Manager)
            return await repo.IsManagerOfAsync(requesterId, userId, ct);
        return false;
    }

    private static bool ValidateLength(SaveCareerPlanRequest req, out string error)
    {
        if (req.LongTermGoal.Length > 1000) { error = "中長期目標は1,000文字以内で入力してください。"; return false; }
        if (req.MediumTermGoal.Length > 1000) { error = "中期目標は1,000文字以内で入力してください。"; return false; }
        if (req.CurrentFocus.Length > 1000) { error = "今期の注力テーマは1,000文字以内で入力してください。"; return false; }
        error = string.Empty;
        return true;
    }

    public async Task<Result<IReadOnlyList<CareerPlanCommentDto>>> GetCommentsAsync(Guid requesterId, Guid careerPlanId, CancellationToken ct = default)
    {
        var plan = await repo.FindAsync(careerPlanId, ct);
        if (plan is null)
            return Result<IReadOnlyList<CareerPlanCommentDto>>.NotFound("キャリアプランが存在しません。");

        if (!await CanReadAsync(requesterId, plan.UserId, ct))
            return Result<IReadOnlyList<CareerPlanCommentDto>>.Forbidden("このキャリアプランを閲覧する権限がありません。");

        var comments = await repo.GetCommentsAsync(careerPlanId, ct);
        return Result<IReadOnlyList<CareerPlanCommentDto>>.Ok(comments.Select(ToCommentDto).ToList());
    }

    public async Task<Result<CareerPlanCommentDto>> AddCommentAsync(Guid requesterId, Guid careerPlanId, AddCommentRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Content))
            return Result<CareerPlanCommentDto>.BadRequest("コメントを入力してください。");

        if (req.Content.Length > 1000)
            return Result<CareerPlanCommentDto>.BadRequest("コメントは1,000文字以内で入力してください。");

        var plan = await repo.FindAsync(careerPlanId, ct);
        if (plan is null)
            return Result<CareerPlanCommentDto>.NotFound("キャリアプランが存在しません。");

        if (!await repo.IsManagerOfAsync(requesterId, plan.UserId, ct))
            return Result<CareerPlanCommentDto>.Forbidden("直属上長のみコメントを投稿できます。");

        var comment = Domain.Entities.CareerPlanComment.Create(careerPlanId, requesterId, req.Content);
        await repo.AddCommentAsync(comment, ct);
        await repo.SaveChangesAsync(ct);

        var author = await repo.FindUserAsync(requesterId, ct);
        return Result<CareerPlanCommentDto>.Ok(new CareerPlanCommentDto(
            comment.Id, comment.CareerPlanId, comment.AuthorId,
            author?.DisplayName ?? string.Empty,
            comment.Content, comment.CreatedAt
        ));
    }

    private static CareerPlanDto ToDto(Domain.Entities.CareerPlan p) => new(
        p.Id, p.UserId, p.PeriodYear,
        p.LongTermGoal, p.MediumTermGoal, p.CurrentFocus,
        p.CreatedAt, p.UpdatedAt
    );

    private static CareerPlanCommentDto ToCommentDto(Domain.Entities.CareerPlanComment c) => new(
        c.Id, c.CareerPlanId, c.AuthorId,
        c.Author?.DisplayName ?? string.Empty,
        c.Content, c.CreatedAt
    );
}
