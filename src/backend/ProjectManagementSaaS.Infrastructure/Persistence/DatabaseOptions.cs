using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSaaS.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool ApplyMigrationsOnStartup { get; init; }

    [Range(1, 180)]
    public int CommandTimeoutSeconds { get; init; } = 30;

    [Range(0, 10)]
    public int MaxRetryCount { get; init; } = 5;

    [Range(1, 120)]
    public int MaxRetryDelaySeconds { get; init; } = 30;
}
