using Microsoft.AspNetCore.Mvc;
using TeckTalentManagement.Application.NineBox;
using TeckTalentManagement.Application.NineBox.Requests;

namespace TeckTalentManagement.Api.Controllers;

[ApiController]
[Route("api/nine-box")]
public class NineBoxController(INineBoxService nineBoxService) : ControllerBase
{
    // 仮のアセッサーID: 認証実装（TASK-002）まで固定値を使用
    private static readonly Guid MockAssessorId = Guid.Parse("00000000-0000-0000-0000-000000000011");

    [HttpPost]
    public async Task<IActionResult> CreateAssessment([FromBody] CreateNineBoxAssessmentRequest req, CancellationToken ct)
    {
        var result = await nineBoxService.CreateAssessmentAsync(MockAssessorId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return StatusCode(201, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAssessment(Guid id, [FromBody] UpdateNineBoxAssessmentRequest req, CancellationToken ct)
    {
        var result = await nineBoxService.UpdateAssessmentAsync(MockAssessorId, id, req, ct);
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
        var result = await nineBoxService.FlagHighPotentialCandidatesAsync(req.PeriodId, MockAssessorId, ct);
        return result.IsSuccess ? Ok(new { changedCount = result.Value }) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public record FlagCandidatesRequest(Guid PeriodId);
