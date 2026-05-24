using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.CompetencyEvaluation;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/competency-evaluations")]
public class CompetencyEvaluationsController(ICompetencyEvaluationService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SaveEvaluations(
        [FromQuery] Guid requesterId,
        [FromBody] SaveCompetencyEvaluationsRequest req,
        CancellationToken ct)
    {
        var result = await service.SaveEvaluationsAsync(requesterId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetEvaluations(
        [FromQuery] Guid requesterId,
        [FromQuery] Guid talentId,
        [FromQuery] Guid periodId,
        CancellationToken ct)
    {
        var result = await service.GetEvaluationsAsync(requesterId, talentId, periodId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
