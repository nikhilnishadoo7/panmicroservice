using System.Text;
using System.Text.Json;
using PAN.API.Infrastructure.Logging;
using PAN.API.Utilities;

namespace PAN.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {

        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "";

        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            SafeLogger.Error(ex, ex.Message, context);

            context.Response.StatusCode = ex.HttpStatus;
            context.Response.ContentType = "application/json";

            var response = ResponseBuilder.Error(ex.Code, ex.Message, correlationId);

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response)
            );
        }
        catch (Exception ex)
        {
            SafeLogger.Error(ex, "Unhandled exception occurred", context);

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var response = ResponseBuilder.ServerError(correlationId);

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response)
            );
        }
    }
}