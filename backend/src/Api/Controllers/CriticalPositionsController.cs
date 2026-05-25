using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.Succession;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/critical-positions")]
public class CriticalPositionsController(ICriticalPositionService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid requesterId,
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var result = await service.GetListAsync(requesterId, includeInactive, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        [FromBody] CreateCriticalPositionRequest req,
        CancellationToken ct = default)
    {
        var result = await service.CreateAsync(requesterId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return StatusCode(201, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        [FromBody] UpdateCriticalPositionRequest req,
        CancellationToken ct = default)
    {
        var result = await service.UpdateAsync(requesterId, id, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
