using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.SkillMigration;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/admin/skill-migration")]
public class SkillMigrationController(ISkillMigrationService service) : ControllerBase
{
    [HttpGet("preview")]
    public async Task<IActionResult> GetPreview(
        [FromQuery] Guid requesterId,
        CancellationToken ct)
    {
        var result = await service.GetPreviewAsync(requesterId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("execute")]
    public async Task<IActionResult> Execute(
        [FromQuery] Guid requesterId,
        CancellationToken ct)
    {
        var result = await service.ExecuteAsync(requesterId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
