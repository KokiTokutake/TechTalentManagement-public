using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TeckTalentManagement.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var report = await healthCheckService.CheckHealthAsync();
        var result = new
        {
            status = report.Status.ToString().ToLower(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString().ToLower()
            })
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(result)
            : StatusCode(503, result);
    }
}
