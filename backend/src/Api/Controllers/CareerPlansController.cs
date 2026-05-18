using Microsoft.AspNetCore.Mvc;
using TeckTalentManagement.Application.CareerPlan;

namespace TeckTalentManagement.Api.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/career-plans")]
public class CareerPlansController(ICareerPlanService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCareerPlan(Guid userId, [FromQuery] Guid requesterId, [FromQuery] int? year, CancellationToken ct)
    {
        var result = await service.GetAsync(requesterId, userId, year, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("years")]
    public async Task<IActionResult> GetYears(Guid userId, [FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.GetYearsAsync(requesterId, userId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("integrated-view")]
    public async Task<IActionResult> GetIntegratedView(Guid userId, [FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.GetIntegratedViewAsync(requesterId, userId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCareerPlan(Guid userId, [FromQuery] Guid requesterId, [FromBody] SaveCareerPlanRequest req, CancellationToken ct)
    {
        var result = await service.CreateAsync(requesterId, userId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return CreatedAtAction(nameof(GetCareerPlan), new { userId }, result.Value);
    }
}

[ApiController]
[Route("api/career-plans")]
public class CareerPlanItemController(ICareerPlanService service) : ControllerBase
{
    [HttpPut("{careerPlanId:guid}")]
    public async Task<IActionResult> UpdateCareerPlan(Guid careerPlanId, [FromQuery] Guid requesterId, [FromBody] SaveCareerPlanRequest req, CancellationToken ct)
    {
        var result = await service.UpdateAsync(requesterId, careerPlanId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("{careerPlanId:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid careerPlanId, [FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.GetCommentsAsync(requesterId, careerPlanId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("{careerPlanId:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid careerPlanId, [FromQuery] Guid requesterId, [FromBody] AddCommentRequest req, CancellationToken ct)
    {
        var result = await service.AddCommentAsync(requesterId, careerPlanId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return CreatedAtAction(nameof(GetComments), new { careerPlanId }, result.Value);
    }
}
