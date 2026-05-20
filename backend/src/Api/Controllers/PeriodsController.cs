using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeckTalentManagement.Infrastructure.Persistence;

namespace TeckTalentManagement.Api.Controllers;

[ApiController]
[Route("api/periods")]
public class PeriodsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var periods = await db.EvaluationPeriods
            .OrderByDescending(p => p.Year)
            .ToListAsync(ct);

        var result = periods.Select(p => new
        {
            p.Id,
            p.Year,
            p.Name,
            p.GoalStartDate,
            p.GoalEndDate,
            p.EvaluationStartDate,
            p.EvaluationEndDate,
            p.ApprovalDeadline,
            p.IsActive,
            CurrentPhase = ComputePhase(p.GoalStartDate, p.GoalEndDate, p.EvaluationStartDate, p.EvaluationEndDate, p.ApprovalDeadline, today),
        });

        return Ok(result);
    }

    private static string ComputePhase(
        DateOnly goalStart, DateOnly goalEnd,
        DateOnly evalStart, DateOnly evalEnd,
        DateOnly approvalDeadline,
        DateOnly today)
    {
        if (today < goalStart) return "Upcoming";
        if (today <= goalEnd) return "GoalRegistration";
        if (today < evalStart) return "Upcoming";
        if (today <= evalEnd) return "SelfEvaluation";
        if (today <= approvalDeadline) return "ManagerEvaluation";
        return "Closed";
    }
}
