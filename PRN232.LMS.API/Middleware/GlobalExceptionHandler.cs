using Microsoft.AspNetCore.Diagnostics;
using PRN232.LMS.Services.ResponseModels;
using System.Net;
using System.Text.Json;

namespace PRN232.LMS.API.Middleware;

/// <summary>
/// Catches all unhandled exceptions and returns a consistent HTTP 500 response
/// in the same ApiResponse format used by all other endpoints.
/// </summary>
public static class GlobalExceptionHandler
{
    public static void UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var error   = feature?.Error;

                // Log to console (replace with ILogger in production)
                Console.Error.WriteLine($"[500] Unhandled exception: {error?.Message}\n{error?.StackTrace}");

                var response = ApiResponse<object>.Fail(
                    "An unexpected internal server error occurred.",
                    new List<string> { error?.Message ?? "Unknown error." });

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(json);
            });
        });
    }
}
