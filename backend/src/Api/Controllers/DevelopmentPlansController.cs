using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.DevelopmentPlan;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/key-persons/{keyPersonId:guid}/development-plans")]
public class DevelopmentPlansController(IDevelopmentPlanService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(Guid keyPersonId, [FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.GetListAsync(requesterId, keyPersonId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid keyPersonId, [FromQuery] Guid requesterId, [FromBody] SaveDevelopmentPlanRequest req, CancellationToken ct)
    {
        var result = await service.AddAsync(requesterId, keyPersonId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return CreatedAtAction(nameof(GetList), new { keyPersonId }, result.Value);
    }
}
