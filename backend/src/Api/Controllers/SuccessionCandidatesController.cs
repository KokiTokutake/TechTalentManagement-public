using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.Succession;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
public class SuccessionCandidatesController(ISuccessionCandidateService service) : ControllerBase
{
    // GET /api/critical-positions/{positionId}/candidates
    [HttpGet("api/critical-positions/{positionId:guid}/candidates")]
    public async Task<IActionResult> GetCandidates(
        Guid positionId,
        [FromQuery] Guid requesterId,
        CancellationToken ct = default)
    {
        var result = await service.GetCandidatesAsync(requesterId, positionId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // POST /api/critical-positions/{positionId}/candidates
    [HttpPost("api/critical-positions/{positionId:guid}/candidates")]
    public async Task<IActionResult> AddCandidate(
        Guid positionId,
        [FromHeader(Name = "X-User-Id")] Guid nominatedBy,
        [FromBody] AddCandidateRequest req,
        CancellationToken ct = default)
    {
        var result = await service.AddCandidateAsync(nominatedBy, positionId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return StatusCode(201, result.Value);
    }

    // PUT /api/succession-candidates/{id}
    [HttpPut("api/succession-candidates/{id:guid}")]
    public async Task<IActionResult> UpdateCandidate(
        Guid id,
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        [FromBody] UpdateCandidateRequest req,
        CancellationToken ct = default)
    {
        var result = await service.UpdateCandidateAsync(requesterId, id, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // DELETE /api/succession-candidates/{id}
    [HttpDelete("api/succession-candidates/{id:guid}")]
    public async Task<IActionResult> RemoveCandidate(
        Guid id,
        [FromQuery] Guid requesterId,
        CancellationToken ct = default)
    {
        var result = await service.RemoveCandidateAsync(requesterId, id, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // POST /api/succession-candidates/{id}/development-actions
    [HttpPost("api/succession-candidates/{id:guid}/development-actions")]
    public async Task<IActionResult> AddDevelopmentAction(
        Guid id,
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        [FromBody] AddDevelopmentActionRequest req,
        CancellationToken ct = default)
    {
        var result = await service.AddDevelopmentActionAsync(requesterId, id, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });
        return StatusCode(201, result.Value);
    }
}

[ApiController]
[Route("api/succession-development-actions")]
public class SuccessionDevelopmentActionsController(ISuccessionCandidateService service) : ControllerBase
{
    // PUT /api/succession-development-actions/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDevelopmentAction(
        Guid id,
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        [FromBody] UpdateDevelopmentActionRequest req,
        CancellationToken ct = default)
    {
        var result = await service.UpdateDevelopmentActionAsync(requesterId, id, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
