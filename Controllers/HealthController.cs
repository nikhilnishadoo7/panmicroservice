using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PAN.API.Application.Services.Interfaces;
using PAN.API.Infrastructure.Logging;

namespace PAN.API.Controllers;

[ApiController]
[Route("api/v1/pan")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;

    public HealthController(IHealthService healthService)
    {
        _healthService = healthService;
    }

    // ✅ Liveness
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        SafeLogger.App("Health Controller Hit");

        var result = await _healthService.GetHealthAsync();

        // ✅ ADD RESPONSE LOG
        SafeLogger.Response(JsonConvert.SerializeObject(result));

        return Ok(result);
    }

    // ✅ Readiness (DB check)
    [HttpGet("health/database")]
    public async Task<IActionResult> HealthReady()
    {
        SafeLogger.App("Health Ready Controller Hit");

        var result = await _healthService.GetHealthReadyAsync();

        // ✅ ADD RESPONSE LOG
        SafeLogger.Response(JsonConvert.SerializeObject(result));

        if (result.Status == "Unhealthy")
        {
            return StatusCode(500, result);
        }

        return Ok(result);
    }
}