using System.Text;
using System.Text.Json;
using PAN.API.Infrastructure.Logging;

namespace PAN.API.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        Stream? originalBody = null;

        try
        {
            var correlationId = Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;

            // -------- Read Request --------
            context.Request.EnableBuffering();

            string requestBody = "";

            if (context.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true
                );

                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            object? requestData = null;

            if (!string.IsNullOrWhiteSpace(requestBody))
            {
                try
                {
                    requestData = JsonSerializer.Deserialize<object>(requestBody);
                }
                catch
                {
                    requestData = requestBody;
                }
            }

            SafeLogger.Request(JsonSerializer.Serialize(new
            {
                correlationId,
                endpoint = context.Request.Path.Value,
                method = context.Request.Method,
                body = requestData
            }), correlationId);

            // -------- Capture Response --------
            originalBody = context.Response.Body;

            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await _next(context);

            // -------- Read Response --------
            memStream.Position = 0;
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();

            object? body = null;

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                if (context.Response.ContentType != null &&
                    context.Response.ContentType.Contains("application/json"))
                {
                    try
                    {
                        body = JsonSerializer.Deserialize<object>(responseBody);
                    }
                    catch
                    {
                        body = responseBody; // fallback
                    }
                }
                else
                {
                    body = responseBody; // keep as string
                }
            }

            SafeLogger.Response(JsonSerializer.Serialize(new
            {
                correlationId,
                statusCode = context.Response.StatusCode,
                body = body
            }), correlationId);

            // -------- Send response back --------
            memStream.Position = 0;
            await memStream.CopyToAsync(originalBody);
        }
        catch (Exception ex)
        {
            SafeLogger.Error(ex, "Middleware error", context);
            throw;
        }
        finally
        {
            if (originalBody != null)
            {
                context.Response.Body = originalBody;
            }
        }
    }
}