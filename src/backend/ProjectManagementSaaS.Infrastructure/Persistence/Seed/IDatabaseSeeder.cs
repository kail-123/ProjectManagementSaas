namespace ProjectManagementSaaS.Infrastructure.Persistence.Seed;

public interface IDatabaseSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}
