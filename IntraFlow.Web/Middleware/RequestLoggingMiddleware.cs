using System.Diagnostics;

namespace IntraFlow.Web.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestId = Activity.Current?.Id ?? context.TraceIdentifier;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["Path"] = context.Request.Path
        }))
        {
            _logger.LogInformation("Request started");

            try
            {
                await _next(context);

                _logger.LogInformation("Request completed with status {StatusCode}",
                    context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                throw;
            }
        }
    }
}