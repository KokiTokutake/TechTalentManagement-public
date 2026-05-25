using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.Succession;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/talents")]
public class TalentsController(ICriticalPositionService criticalPositionService) : ControllerBase
{
    [HttpGet("{talentId:guid}/succession-candidacies")]
    public async Task<IActionResult> GetSuccessionCandidacies(
        Guid talentId,
        [FromQuery] Guid requesterId,
        CancellationToken ct = default)
    {
        var result = await criticalPositionService.GetTalentCandidaciesAsync(requesterId, talentId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
