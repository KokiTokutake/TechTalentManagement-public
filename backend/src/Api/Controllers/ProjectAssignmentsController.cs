using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.ProjectAssignment;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/project-assignments")]
public class ProjectAssignmentsController(IProjectAssignmentService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        [FromQuery] Guid talentId,
        CancellationToken ct = default)
    {
        if (talentId == Guid.Empty)
            return BadRequest(new { error = "talentId は必須です。" });

        var result = await service.GetListAsync(requesterId, talentId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        [FromBody] CreateProjectAssignmentRequest req,
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
        [FromBody] UpdateProjectAssignmentRequest req,
        CancellationToken ct = default)
    {
        var result = await service.UpdateAsync(requesterId, id, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        CancellationToken ct = default)
    {
        var result = await service.DeleteAsync(requesterId, id, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return NoContent();
    }

    [HttpPut("{id:guid}/manager-comment")]
    public async Task<IActionResult> SaveManagerComment(
        Guid id,
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        [FromBody] SaveManagerCommentRequest req,
        CancellationToken ct = default)
    {
        var result = await service.SaveManagerCommentAsync(requesterId, id, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
