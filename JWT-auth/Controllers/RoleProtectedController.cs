using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWT_auth.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleProtectedController : ControllerBase
{
    [HttpGet("individual")]
    [Authorize(Roles = "Individual")]
    public IActionResult IndividualEndpoint() => Ok("Individual role access granted.");

    [HttpGet("corporate")]
    [Authorize(Roles = "Corporate")]
    public IActionResult CorporateEndpoint() => Ok("Corporate role access granted.");

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminEndpoint() => Ok("Admin role access granted.");
}
