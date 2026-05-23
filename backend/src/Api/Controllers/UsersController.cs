using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Infrastructure.Persistence.Repositories;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await repo.GetAllAsync(ct);
        return Ok(users);
    }
}
