using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.GoalKpi;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/goals/{goalId:guid}")]
public class GoalKpiController(IGoalKpiService service) : ControllerBase
{
    private Guid RequesterId => Guid.TryParse(Request.Headers["X-User-Id"], out var id) ? id : Guid.Empty;

    [HttpPost("kpi")]
    public async Task<IActionResult> CreateKpi(Guid goalId, [FromBody] SaveGoalKpiRequest req, CancellationToken ct)
    {
        var result = await service.SaveKpiAsync(RequesterId, goalId, req, isUpdate: false, ct);
        return result.IsSuccess
            ? StatusCode(201, result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPut("kpi")]
    public async Task<IActionResult> UpdateKpi(Guid goalId, [FromBody] SaveGoalKpiRequest req, CancellationToken ct)
    {
        var result = await service.SaveKpiAsync(RequesterId, goalId, req, isUpdate: true, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("checkins")]
    public async Task<IActionResult> CreateCheckin(Guid goalId, [FromBody] CreateCheckinRequest req, CancellationToken ct)
    {
        var result = await service.CreateCheckinAsync(RequesterId, goalId, req, ct);
        return result.IsSuccess
            ? StatusCode(201, result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("checkins")]
    public async Task<IActionResult> GetCheckins(Guid goalId, CancellationToken ct)
    {
        var result = await service.GetCheckinsAsync(RequesterId, goalId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
