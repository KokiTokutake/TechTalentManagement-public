using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.SkillGap;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/skills/gap")]
public class SkillGapController(ISkillGapService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetGap(
        [FromQuery] Guid requesterId,
        [FromQuery] Guid talentId,
        [FromQuery] Guid periodId,
        [FromQuery] int threshold = 2,
        CancellationToken ct = default)
    {
        var result = await service.GetGapAsync(requesterId, talentId, periodId, threshold, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
