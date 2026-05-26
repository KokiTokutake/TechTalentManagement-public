using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.ProjectAssignment;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class ProjectAssignmentRepository(AppDbContext db) : IProjectAssignmentRepository
{
    public Task<User?> FindUserAsync(Guid userId, CancellationToken ct) =>
        db.Users.FindAsync([userId], ct).AsTask();

    public async Task<IReadOnlyList<ProjectAssignmentDto>> GetListByTalentIdAsync(Guid talentId, CancellationToken ct)
    {
        var assignments = await db.ProjectAssignments
            .Where(a => a.TalentId == talentId)
            .Include(a => a.Skills)
                .ThenInclude(s => s.SkillItem)
                    .ThenInclude(si => si.Category)
            .ToListAsync(ct);

        // EndDate IS NULL を先頭、その後 StartDate DESC
        return assignments
            .OrderBy(a => a.EndDate.HasValue ? 1 : 0)
            .ThenByDescending(a => a.StartDate)
            .Select(a => new ProjectAssignmentDto(
                a.Id,
                a.ProjectName,
                a.Role,
                a.StartDate,
                a.EndDate,
                IsActive: !a.EndDate.HasValue,
                a.Description,
                a.ManagerComment,
                a.Skills.Select(s => new ProjectAssignmentSkillDto(
                    s.SkillItemId,
                    s.SkillItem.Name,
                    s.SkillItem.Category?.Name ?? string.Empty
                )).ToList(),
                a.CreatedAt,
                a.UpdatedAt
            )).ToList();
    }

    public Task<Domain.Entities.ProjectAssignment?> FindByIdWithSkillsAsync(Guid id, CancellationToken ct) =>
        db.ProjectAssignments
            .Include(a => a.Skills)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task AddAsync(Domain.Entities.ProjectAssignment assignment, CancellationToken ct) =>
        await db.ProjectAssignments.AddAsync(assignment, ct);

    public async Task<bool> SkillItemsExistAsync(IEnumerable<Guid> skillItemIds, CancellationToken ct)
    {
        var ids = skillItemIds.ToList();
        var count = await db.SkillItems.CountAsync(s => ids.Contains(s.Id), ct);
        return count == ids.Count;
    }

    public void Delete(Domain.Entities.ProjectAssignment assignment) =>
        db.ProjectAssignments.Remove(assignment);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
