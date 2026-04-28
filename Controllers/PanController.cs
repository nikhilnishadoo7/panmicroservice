using Microsoft.AspNetCore.Mvc;
using PAN.API.Application.DTOs.Request;
using PAN.API.Application.Services.Interfaces;
using PAN.API.Infrastructure.Logging;
using PAN.API.Utilities;

namespace PAN.API.Controllers;

[ApiController]
[Route("api/v1/pan")]
public class PanController : ControllerBase
{
    private readonly IPanVerificationService _service;

    public PanController(IPanVerificationService service)
    {
        _service = service;
    }
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] PanRequest? request)
    
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

        if (request == null)
            return BadRequest(ResponseBuilder.InvalidRequest("Request body missing", correlationId));

        if (string.IsNullOrWhiteSpace(request.IdNumber))
            return BadRequest(ResponseBuilder.InvalidRequest("PAN is required", correlationId));

        if (!ValidationHelper.IsValidPan(request.IdNumber))
            return BadRequest(ResponseBuilder.InvalidPanFormat(correlationId));

        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
         ?? HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString()
         ?? "UNKNOWN";

        var result = await _service.PanVerifyAsync(request, correlationId, ip);

        if (result.IsSuccess)
            return Ok(ResponseBuilder.PanVerified(result, correlationId));
        else
            return BadRequest(ResponseBuilder.PanInvalid(result, correlationId));
    }
}