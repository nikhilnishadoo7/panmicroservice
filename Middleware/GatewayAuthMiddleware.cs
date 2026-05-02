namespace pan.Middleware
{
    public class GatewayAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public GatewayAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip health and swagger — no auth needed on these
            if (context.Request.Path.StartsWithSegments("/health") ||
context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            // APISix validates the JWT and injects this header on every forwarded request
            // If it is missing the request came directly to the microservice port — block it
            var consumer = context.Request.Headers["X-Consumer-Username"].FirstOrDefault();

            if (string.IsNullOrEmpty(consumer))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                "{\"error\":\"Unauthorized\",\"message\":\"All requests must go through the API gateway\"}");
                return;
            }

            // Make consumer identity available to controllers
            context.Items["ConsumerUsername"] = consumer;
            context.Items["AppId"] = consumer.Split('_')[0];
            context.Items["Environment"] = string.Join("_", consumer.Split('_').Skip(1));

            await _next(context);
        }
    }
}
