using SoccerPro.Domain;

namespace SoccerPro.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionHandlingMiddleware> logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(
                exception,
                "Unhandled exception processing {Method} {Path} with correlation {CorrelationId} and trace {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.Headers["CorrelationId"].FirstOrDefault(),
                System.Diagnostics.Activity.Current?.TraceId.ToString());

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new Error("An unexpected error occurred.", "unexpected_error"));
            }
        }
    }
}
