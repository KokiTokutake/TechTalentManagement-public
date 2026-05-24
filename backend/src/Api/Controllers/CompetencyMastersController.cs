using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.CompetencyMaster;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/competency-masters")]
public class CompetencyMastersController(ICompetencyMasterService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCompetencies(CancellationToken ct)
    {
        var result = await service.GetActiveCompetenciesAsync(ct);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCompetency(
        [FromQuery] Guid requesterId,
        [FromBody] CreateCompetencyMasterRequest req,
        CancellationToken ct)
    {
        var result = await service.CreateCompetencyAsync(requesterId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return StatusCode(201, result.Value);
    }

    [HttpPut("{competencyId:guid}")]
    public async Task<IActionResult> UpdateCompetency(
        Guid competencyId,
        [FromQuery] Guid requesterId,
        [FromBody] UpdateCompetencyMasterRequest req,
        CancellationToken ct)
    {
        var result = await service.UpdateCompetencyAsync(requesterId, competencyId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
