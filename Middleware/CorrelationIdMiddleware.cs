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
            
            var path = context.Request.Path.Value;
            if (path != null && path.StartsWith("/swagger"))
            {
                await _next(context);
                return;
            }

            var correlationId = Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;

           
            context.Request.EnableBuffering();

            string requestBody = "";

            try
            {
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
            }
            catch (Exception ex)
            {
                SafeLogger.Error(ex, "Error reading request body", context);
            }

            var requestLog = JsonSerializer.Serialize(new
            {
                correlationId,
                endpoint = context.Request.Path.Value,
                method = context.Request.Method,
                body = string.IsNullOrWhiteSpace(requestBody)
                            ? null
                            : JsonSerializer.Deserialize<object>(requestBody)
            });

            SafeLogger.Request(requestLog, correlationId);

            
            originalBody = context.Response.Body;

            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await _next(context);

            memStream.Position = 0;
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();

            var responseLog = JsonSerializer.Serialize(new
            {
                correlationId,
                statusCode = context.Response.StatusCode,
                body = string.IsNullOrWhiteSpace(responseBody)
                            ? null
                            : JsonSerializer.Deserialize<object>(responseBody)
            });

            SafeLogger.Response(responseLog, correlationId);

            // Write back to original stream
            memStream.Position = 0;
            await memStream.CopyToAsync(originalBody);
        }
        catch (Exception ex)
        {
            SafeLogger.Error(ex, "Unhandled error in CorrelationIdMiddleware", context);
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