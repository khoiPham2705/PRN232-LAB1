using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace PRN232.LMS.API.Middleware;

/// <summary>
/// Catches all unhandled exceptions globally and returns a consistent HTTP 500 response.
/// Avoids exposing internal details and returns errors as null, supporting Content Negotiation.
/// </summary>
public static class GlobalExceptionHandler
{
    public static void UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var error = feature?.Error;

                // Log the exception details internally
                Console.Error.WriteLine($"[500] Unhandled exception: {error?.Message}\n{error?.StackTrace}");

                var acceptHeader = context.Request.Headers["Accept"].ToString();
                bool wantsXml = acceptHeader.Contains("application/xml", StringComparison.OrdinalIgnoreCase);

                if (wantsXml)
                {
                    context.Response.ContentType = "application/xml";
                    var xml = """
                              <?xml version="1.0" encoding="utf-8"?>
                              <ApiResponseOfObject xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                                <Success>false</Success>
                                <Message>Internal server error</Message>
                                <Errors xsi:nil="true" />
                                <Data xsi:nil="true" />
                              </ApiResponseOfObject>
                              """;
                    await context.Response.WriteAsync(xml);
                }
                else
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        success = false,
                        message = "Internal server error",
                        errors = (object?)null,
                        data = (object?)null
                    };
                    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    await context.Response.WriteAsync(json);
                }
            });
        });
    }
}
