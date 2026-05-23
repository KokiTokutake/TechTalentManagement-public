using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.CareerValues;
using TechTalentManagement.Application.Wcm;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
public class WcmController(IWcmService wcmService, ICareerValuesService valuesService) : ControllerBase
{
    // GET /api/career/wcm?talentId={guid}&periodId={guid}
    [HttpGet("api/career/wcm")]
    public async Task<IActionResult> GetWcm([FromQuery] Guid talentId, [FromQuery] Guid periodId, CancellationToken ct)
    {
        var result = await wcmService.GetWcmAsync(GetCurrentUserId(), talentId, periodId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // PUT /api/career/wcm
    [HttpPut("api/career/wcm")]
    public async Task<IActionResult> SaveWcm([FromBody] SaveWcmRequest req, CancellationToken ct)
    {
        var result = await wcmService.SaveWcmAsync(GetCurrentUserId(), req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // GET /api/career/values?talentId={guid}&periodId={guid}
    [HttpGet("api/career/values")]
    public async Task<IActionResult> GetValues([FromQuery] Guid talentId, [FromQuery] Guid periodId, CancellationToken ct)
    {
        var result = await valuesService.GetValuesAsync(GetCurrentUserId(), talentId, periodId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // PUT /api/career/values
    [HttpPut("api/career/values")]
    public async Task<IActionResult> SaveValues([FromBody] SaveCareerValuesRequest req, CancellationToken ct)
    {
        var result = await valuesService.SaveValuesAsync(GetCurrentUserId(), req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    private Guid GetCurrentUserId()
    {
        var header = Request.Headers["X-User-Id"].FirstOrDefault();
        return Guid.TryParse(header, out var id) ? id : Guid.Empty;
    }
}
