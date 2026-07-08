using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProjectManagementSaaS.Api.Extensions;

public static class HealthCheckEndpointExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static IEndpointRouteBuilder MapHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteHealthResponseAsync
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready", StringComparer.OrdinalIgnoreCase),
            ResponseWriter = WriteHealthResponseAsync
        });

        return endpoints;
    }

    private static async Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration,
            entries = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration,
                    description = entry.Value.Description
                },
                StringComparer.Ordinal)
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, SerializerOptions), context.RequestAborted);
    }
}
