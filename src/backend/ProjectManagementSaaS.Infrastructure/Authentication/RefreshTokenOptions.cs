using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSaaS.Infrastructure.Authentication;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "Authentication:RefreshTokens";

    [Range(1, 365)]
    public int ExpirationDays { get; init; } = 14;

    [Range(1, 100)]
    public int MaxActiveTokensPerUser { get; init; } = 10;

    [Required]
    [MinLength(3)]
    public string CookieName { get; init; } = "__Host-pmsa-refresh";

    [Required]
    public string CookieSameSite { get; init; } = "Strict";

    public bool CookieSecure { get; init; } = true;
}
