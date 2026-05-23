using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.CommitteeSession;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/committee-sessions")]
public class CommitteeSessionsController(ICommitteeSessionService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.GetListAsync(requesterId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] Guid requesterId, [FromBody] CreateCommitteeSessionRequest req, CancellationToken ct)
    {
        var result = await service.CreateAsync(requesterId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.GetAsync(requesterId, id, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
