using Microsoft.AspNetCore.Mvc;
using TeckTalentManagement.Application.Goals;

namespace TeckTalentManagement.Api.Controllers;

[ApiController]
[Route("api/periods/{periodId:guid}/users/{userId:guid}/goals")]
public class GoalsController(IGoalService goalService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetGoals(Guid periodId, Guid userId, CancellationToken ct)
    {
        var result = await goalService.GetGoalsAsync(userId, periodId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> CreateGoal(Guid periodId, Guid userId, [FromBody] CreateGoalRequest req, CancellationToken ct)
    {
        var result = await goalService.CreateGoalAsync(userId, periodId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return CreatedAtAction(nameof(GetGoals), new { periodId, userId }, result.Value);
    }

    [HttpPut("{goalId:guid}")]
    public async Task<IActionResult> UpdateGoal(Guid periodId, Guid userId, Guid goalId, [FromBody] UpdateGoalRequest req, CancellationToken ct)
    {
        var result = await goalService.UpdateGoalAsync(userId, goalId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpDelete("{goalId:guid}")]
    public async Task<IActionResult> DeleteGoal(Guid periodId, Guid userId, Guid goalId, CancellationToken ct)
    {
        var result = await goalService.DeleteGoalAsync(userId, goalId, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
