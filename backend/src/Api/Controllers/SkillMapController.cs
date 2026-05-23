using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.SkillMap;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/skill-map")]
public class SkillMapController(ISkillMapService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSkillMap([FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.GetSkillMapAsync(requesterId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
