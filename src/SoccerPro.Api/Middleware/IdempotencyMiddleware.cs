using System.Security.Cryptography;
using System.Text;
using SoccerPro.Application;
using SoccerPro.Domain;

namespace SoccerPro.Api.Middleware;

public sealed class IdempotencyMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<IdempotencyMiddleware> logger;

    public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IIdempotencyStore store)
    {
        if (!HttpMethods.IsPost(context.Request.Method) ||
            !context.Request.Headers.TryGetValue("Idempotency-Key", out var key) ||
            string.IsNullOrWhiteSpace(key))
        {
            await next(context);
            return;
        }

        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, false, 1024, true);
        var requestBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        var requestHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(context.Request.Path + ":" + requestBody)));
        var existing = await store.GetAsync(key.ToString(), context.RequestAborted);
        if (existing is not null)
        {
            if (existing.RequestHash != requestHash)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new Error("Idempotency key was used with a different request.", "conflict"));
                return;
            }

            context.Response.StatusCode = existing.StatusCode;
            if (existing.Location is not null)
                context.Response.Headers["Location"] = existing.Location;
            await context.Response.WriteAsync(existing.ResponseBody);
            return;
        }

        var originalBody = context.Response.Body;
        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;
        try
        {
            await next(context);
            responseBuffer.Position = 0;
            var responseBody = await new StreamReader(responseBuffer).ReadToEndAsync();
            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            if (context.Response.StatusCode is >= 200 and < 300)
            {
                var saveResult = await store.SaveAsync(new IdempotencyRecord
                {
                    Key = key.ToString(),
                    RequestHash = requestHash,
                    StatusCode = context.Response.StatusCode,
                    ResponseBody = responseBody,
                    Location = context.Response.Headers["Location"].FirstOrDefault()
                }, context.RequestAborted);

                if (!saveResult.IsSuccess)
                    logger.LogError("Idempotency response could not be persisted for key {IdempotencyKey}", key.ToString());
            }
        }
        catch
        {
            context.Response.Body = originalBody;
            throw;
        }
    }
}
