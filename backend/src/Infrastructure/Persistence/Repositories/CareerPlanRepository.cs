using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Application.CareerPlan;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Infrastructure.Persistence.Repositories;

public class CareerPlanRepository(AppDbContext db) : ICareerPlanRepository
{
    public Task<CareerPlan?> FindByUserAndYearAsync(Guid userId, int year, CancellationToken ct) =>
        db.CareerPlans
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.PeriodYear == year, ct);

    public Task<CareerPlan?> FindAsync(Guid id, CancellationToken ct) =>
        db.CareerPlans.FindAsync([id], ct).AsTask();

    public async Task<IReadOnlyList<int>> GetYearsAsync(Guid userId, CancellationToken ct) =>
        await db.CareerPlans
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PeriodYear)
            .Select(p => p.PeriodYear)
            .ToListAsync(ct);

    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FindAsync([userId], ct).AsTask();

    public Task<bool> IsManagerOfAsync(Guid managerId, Guid userId, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Id == userId && u.ManagerId == managerId, ct);

    public async Task AddAsync(CareerPlan plan, CancellationToken ct) =>
        await db.CareerPlans.AddAsync(plan, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<UserSkill>> GetApprovedSkillsAsync(Guid userId, CancellationToken ct) =>
        await db.UserSkills
            .Include(s => s.SkillItem)
            .ThenInclude(i => i!.Category)
            .Where(s => s.UserId == userId && s.Status == Domain.Enums.UserSkillStatus.Approved)
            .OrderBy(s => s.SkillItem!.Category!.Name)
            .ThenBy(s => s.SkillItem!.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Goal>> GetCurrentYearGoalsAsync(Guid userId, CancellationToken ct)
    {
        var currentYear = DateTime.UtcNow.Year;
        return await db.Goals
            .Include(g => g.EvaluationPeriod)
            .Where(g => g.UserId == userId && g.EvaluationPeriod!.Year == currentYear)
            .OrderBy(g => g.Category)
            .ThenBy(g => g.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CareerPlanComment>> GetCommentsAsync(Guid careerPlanId, CancellationToken ct) =>
        await db.CareerPlanComments
            .Include(c => c.Author)
            .Where(c => c.CareerPlanId == careerPlanId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public async Task AddCommentAsync(CareerPlanComment comment, CancellationToken ct) =>
        await db.CareerPlanComments.AddAsync(comment, ct);

    public async Task<IReadOnlyList<(User User, CareerPlan? Plan)>> GetAllTalentsWithCurrentPlanAsync(CancellationToken ct)
    {
        var currentYear = DateTime.UtcNow.Year;
        var talents = await db.Users
            .Where(u => u.Role == Domain.Enums.UserRole.Talent)
            .OrderBy(u => u.DisplayName)
            .ToListAsync(ct);

        var userIds = talents.Select(u => u.Id).ToList();
        var plans = await db.CareerPlans
            .Where(p => userIds.Contains(p.UserId) && p.PeriodYear == currentYear)
            .ToListAsync(ct);

        var planByUser = plans.ToDictionary(p => p.UserId);
        return talents.Select(u => (u, planByUser.GetValueOrDefault(u.Id))).ToList();
    }
}
