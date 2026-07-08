using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Seed;

public sealed class BootstrapAdminOptions
{
    public bool Enabled { get; init; }

    [EmailAddress]
    public string? Email { get; init; }

    public string? Password { get; init; }

    [MaxLength(256)]
    public string? DisplayName { get; init; }
}
