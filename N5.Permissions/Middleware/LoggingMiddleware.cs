using System.Diagnostics;

namespace N5.Permissions.API.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();

            // Add request ID to context for correlation
            context.Items["RequestId"] = requestId;

            _logger.LogInformation("Request {RequestId} started: {Method} {Path}",
                requestId, context.Request.Method, context.Request.Path);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation("Request {RequestId} completed in {ElapsedMs}ms with status {StatusCode}",
                    requestId, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
            }
        }
    }
}
