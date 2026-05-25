using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.Succession;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/succession-map")]
public class SuccessionMapController(ISuccessionCandidateService service) : ControllerBase
{
    // GET /api/succession-map
    [HttpGet]
    public async Task<IActionResult> GetMap(
        [FromQuery] Guid requesterId,
        CancellationToken ct = default)
    {
        var result = await service.GetSuccessionMapAsync(requesterId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // GET /api/succession-map/suggest
    [HttpGet("suggest")]
    public async Task<IActionResult> GetSuggestions(
        [FromQuery] Guid requesterId,
        [FromQuery] Guid periodId,
        [FromQuery] Guid positionId,
        CancellationToken ct = default)
    {
        var result = await service.GetSuggestionsAsync(requesterId, periodId, positionId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
