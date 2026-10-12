using System.Diagnostics;

namespace SoccerPro.Api.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<RequestLoggingMiddleware> logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["CorrelationId"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
        context.Response.Headers["CorrelationId"] = correlationId;
        var watch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error : LogLevel.Information;
            logger.Log(
                logLevel,
                "HTTP request completed with method {Method}, path {Path}, status {StatusCode}, elapsed {ElapsedMs}ms, correlation {CorrelationId}, trace {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                watch.ElapsedMilliseconds,
                correlationId,
                Activity.Current?.TraceId.ToString());
        }
    }
}
