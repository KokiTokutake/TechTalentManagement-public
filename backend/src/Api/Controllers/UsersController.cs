using Microsoft.AspNetCore.Mvc;
using TeckTalentManagement.Infrastructure.Persistence.Repositories;

namespace TeckTalentManagement.Api.Controllers;

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
