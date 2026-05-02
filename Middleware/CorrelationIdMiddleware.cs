using System.Text;
using System.Text.Json;
using PAN.API.Infrastructure.Logging;
using PAN.API.Utilities;

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

            string maskedBody = MaskingHelper.MaskSensitiveData(requestBody);

            SafeLogger.Request(new
            {
                method = context.Request.Method,
                path = context.Request.Path,
                correlationId,
                body = string.IsNullOrWhiteSpace(maskedBody) ? null : maskedBody
            });

            originalBody = context.Response.Body;

            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await _next(context);

            memStream.Position = 0;
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();

            object? body = null;

            if (!string.IsNullOrWhiteSpace(responseBody) &&
                context.Response.ContentType?.Contains("application/json") == true &&
                responseBody.Length < 5000)
            {
                try
                {
                    body = JsonSerializer.Deserialize<JsonElement>(responseBody);
                }
                catch
                {
                    body = "[non-json response]";
                }
            }
            else if (responseBody.Length >= 5000)
            {
                body = $"[response too large to log: {responseBody.Length} chars]";
            }

            SafeLogger.Response(JsonSerializer.Serialize(new
            {
                correlationId,
                statusCode = context.Response.StatusCode,
                body = body
            }));

            memStream.Position = 0;
            await memStream.CopyToAsync(originalBody);
        }
        catch (Exception ex)
        {
            SafeLogger.Error(ex, "Middleware error", new
            {
                path = context.Request.Path.ToString(),
                method = context.Request.Method,
                correlationId = context.Items["CorrelationId"]?.ToString(),
                statusCode = context.Response?.StatusCode
            });
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