using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.NineBox;
using TechTalentManagement.Application.NineBox.Requests;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/nine-box")]
public class NineBoxController(INineBoxService nineBoxService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAssessment([FromBody] CreateNineBoxAssessmentRequest req, CancellationToken ct)
    {
        var assessorId = GetCurrentUserId();
        var result = await nineBoxService.CreateAssessmentAsync(assessorId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return StatusCode(201, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAssessment(Guid id, [FromBody] UpdateNineBoxAssessmentRequest req, CancellationToken ct)
    {
        var assessorId = GetCurrentUserId();
        var result = await nineBoxService.UpdateAssessmentAsync(assessorId, id, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetMap(
        [FromQuery] Guid periodId,
        [FromQuery] Guid? departmentId,
        [FromQuery] string? role,
        [FromQuery] string? searchName,
        [FromQuery] Guid? comparePeriodId,
        CancellationToken ct)
    {
        var result = await nineBoxService.GetMapAsync(periodId, departmentId, role, searchName, comparePeriodId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("cells")]
    public async Task<IActionResult> GetCells(CancellationToken ct)
    {
        var result = await nineBoxService.GetCellDefinitionsAsync(ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] Guid talentId, CancellationToken ct)
    {
        var result = await nineBoxService.GetHistoryAsync(talentId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("calibration")]
    public async Task<IActionResult> GetCalibration([FromQuery] Guid periodId, CancellationToken ct)
    {
        var result = await nineBoxService.GetCalibrationAsync(periodId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("flag-candidates")]
    public async Task<IActionResult> FlagCandidates([FromBody] FlagCandidatesRequest req, CancellationToken ct)
    {
        var assessorId = GetCurrentUserId();
        var result = await nineBoxService.FlagHighPotentialCandidatesAsync(req.PeriodId, assessorId, ct);
        return result.IsSuccess ? Ok(new { changedCount = result.Value }) : StatusCode(result.StatusCode, new { error = result.Error });
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

public record FlagCandidatesRequest(Guid PeriodId);
