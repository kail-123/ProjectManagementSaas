using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSaaS.Api;

public sealed class ApiCorsOptions
{
    public const string SectionName = "Cors";
    public const string PolicyName = "ConfiguredCorsPolicy";

    [MinLength(1)]
    public string[] AllowedOrigins { get; init; } = [];

    public bool AllowCredentials { get; init; } = true;
}
