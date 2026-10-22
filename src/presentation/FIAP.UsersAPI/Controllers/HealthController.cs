using Microsoft.AspNetCore.Mvc;

namespace FIAP.UsersAPI.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy", service = "UsersAPI" });
}
