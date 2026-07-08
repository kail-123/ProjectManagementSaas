namespace ProjectManagementSaaS.Infrastructure.Persistence.Seed;

public sealed class SeedDataOptions
{
    public const string SectionName = "SeedData";

    public bool RunOnStartup { get; init; }

    public BootstrapAdminOptions BootstrapAdmin { get; init; } = new();
}
