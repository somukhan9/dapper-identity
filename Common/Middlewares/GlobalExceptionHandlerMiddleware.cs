using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Middlewares;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError($"Global Exception MESSAGE :: {ex.Message}");
            logger.LogError($"Global Exception STACKTRACE :: {ex.StackTrace}");
            logger.LogError($"Global Exception :: {ex}");
        }
    }
}