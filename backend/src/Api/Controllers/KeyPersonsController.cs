using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.CommitteeSession;
using TechTalentManagement.Application.KeyPerson;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/key-persons")]
public class KeyPersonsController(IKeyPersonService service, ICommitteeSessionService committeeService) : ControllerBase
{
    [HttpGet("committee-summary")]
    public async Task<IActionResult> GetCommitteeSummary([FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await committeeService.GetCommitteeSummaryAsync(requesterId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.GetListAsync(requesterId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Designate([FromQuery] Guid requesterId, [FromBody] DesignateKeyPersonRequest req, CancellationToken ct)
    {
        var result = await service.DesignateAsync(requesterId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return CreatedAtAction(nameof(GetList), result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, [FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.RevokeAsync(requesterId, id, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyStatus([FromQuery] Guid talentId, CancellationToken ct)
    {
        var result = await service.GetMyStatusAsync(talentId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
