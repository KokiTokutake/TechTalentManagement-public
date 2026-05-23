using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.TalentProfile;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/talents/{talentId:guid}")]
public class TalentProfileController(ITalentProfileService service) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(
        Guid talentId,
        [FromQuery] Guid? periodId,
        CancellationToken ct)
    {
        var requesterId = GetCurrentUserId();
        var result = await service.GetProfileAsync(requesterId, talentId, periodId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPut("manager-comment")]
    public async Task<IActionResult> UpsertManagerComment(
        Guid talentId,
        [FromQuery] Guid periodId,
        [FromBody] UpsertManagerCommentRequest req,
        CancellationToken ct)
    {
        var managerId = GetCurrentUserId();
        var result = await service.UpsertManagerCommentAsync(managerId, talentId, periodId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPatch("key-person-flag")]
    public async Task<IActionResult> ToggleKeyPersonFlag(
        Guid talentId,
        [FromBody] ToggleKeyPersonFlagRequest req,
        CancellationToken ct)
    {
        var requesterId = GetCurrentUserId();
        var result = await service.ToggleKeyPersonFlagAsync(requesterId, talentId, req.IsActive, req.Reason, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // TODO: T-002 認証実装後に ClaimsPrincipal から取得する
    private Guid GetCurrentUserId()
    {
        var header = HttpContext.Request.Headers["X-User-Id"].FirstOrDefault();
        if (header is not null && Guid.TryParse(header, out var id))
            return id;

        return Guid.Parse("00000000-0000-0000-0000-000000000011");
    }
}

public record ToggleKeyPersonFlagRequest(bool IsActive, string? Reason);
