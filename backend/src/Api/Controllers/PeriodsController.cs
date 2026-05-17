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
        var periods = await db.EvaluationPeriods
            .OrderByDescending(p => p.Year)
            .Select(p => new
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
            })
            .ToListAsync(ct);
        return Ok(periods);
    }
}
