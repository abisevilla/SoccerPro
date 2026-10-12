using Microsoft.AspNetCore.Mvc;

namespace SoccerPro.Api.Controllers;

[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy" });
}
