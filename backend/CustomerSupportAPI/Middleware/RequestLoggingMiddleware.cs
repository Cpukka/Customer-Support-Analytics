namespace CustomerSupportAPI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            
            // Log request
            _logger.LogInformation(
                "HTTP {Method} {Path} started at {Time}",
                context.Request.Method,
                context.Request.Path,
                startTime
            );

            try
            {
                await _next(context);
                
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "HTTP {Method} {Path} completed with {StatusCode} in {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    duration.TotalMilliseconds
                );
            }
            catch
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    "HTTP {Method} {Path} failed after {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    duration.TotalMilliseconds
                );
                throw;
            }
        }
    }
}