using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DEPI_Pro.Middleware
{
    /// <summary>
    /// Global exception handling middleware that catches unhandled exceptions,
    /// logs them with a unique Error ID, and redirects the user to a clean error page.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid().ToString("N")[..8].ToUpper();
                _logger.LogError(ex, "Unhandled exception caught. Error ID: {ErrorId} | Path: {Path} | User: {User}",
                    errorId, context.Request.Path, context.User?.Identity?.Name ?? "Anonymous");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // For API requests, return JSON
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.ContentType = "application/json";
                    var json = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = $"An internal error occurred. Please reference Error ID: {errorId} when contacting support.",
                        errorId
                    });
                    await context.Response.WriteAsync(json);
                }
                else
                {
                    // For MVC requests, redirect to error page
                    context.Response.Redirect($"/Home/Error?errorId={errorId}");
                }
            }
        }
    }
}
