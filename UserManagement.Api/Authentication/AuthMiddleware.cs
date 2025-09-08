namespace UserManagement.Api.Authentication;

public class AuthMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value!;

        // Allow unauthenticated access to Swagger and Scalar documentation
        if (path.StartsWith("/swagger") || path.StartsWith("/scalar") || path.StartsWith("/openapi"))
        {
            await next(context);
            return;
        }

        // Check and extract the API key if provided in the request headers
        if (!context.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var providedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = 401,
                Error = "Missing API key.",
                Detail = "Please include the API key in the request header 'x-api-key'."
            });;
            return;
        }

        // Retrieve the API key from the configuration
        var apiKey = configuration.GetValue<string>(AuthConstants.ApiKeySectionName);

        // Compare the provided API key with the server-side API key
        if (apiKey != providedApiKey)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = 401,
                Error = "Invalid API key.",
                Detail = "Please provide a valid API key in the request header 'x-api-key'."
            });
            return;
        }

        await next(context);
    }
}
