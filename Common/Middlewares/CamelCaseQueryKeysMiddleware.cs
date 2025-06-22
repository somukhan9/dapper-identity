using Microsoft.AspNetCore.Http;

namespace Common.Middlewares;

public class CamelCaseQueryKeysMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var originalQuery = context.Request.Query;
        var camelQuery = new QueryString();

        foreach (var kvp in originalQuery)
        {
            camelQuery = camelQuery.Add(ToCamelCase(kvp.Key), kvp.Value!);
        }

        context.Request.QueryString = camelQuery;

        await next(context);
    }

    private string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
        {
            return input;
        }

        return char.ToLowerInvariant(input[0]) + input.Substring(1);
    }
}