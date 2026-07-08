using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ProjectManagementSaaS.Application.Abstractions.Authentication;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Abstractions.Identity;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Infrastructure.Authentication;
using ProjectManagementSaaS.Infrastructure.Authorization;
using ProjectManagementSaaS.Infrastructure.Common;
using ProjectManagementSaaS.Infrastructure.Identity;
using ProjectManagementSaaS.Infrastructure.Persistence;
using ProjectManagementSaaS.Infrastructure.Persistence.Entities;
using ProjectManagementSaaS.Infrastructure.Persistence.Seed;

namespace ProjectManagementSaaS.Infrastructure;

public static class DependencyInjection
{
    private const string DefaultConnectionName = "DefaultConnection";
    private const string IdentitySectionName = "Identity";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatedOptions(configuration);
        services.AddPersistence(configuration);
        services.AddIdentityServices(configuration);
        services.AddAuthenticationServices();
        services.AddAuthorizationServices();
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "sqlserver",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"]);

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IPaginationService, EfPaginationService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IUserPermissionService, UserPermissionService>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();

        return services;
    }

    private static void AddValidatedOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetSection(RefreshTokenOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => IsSupportedSameSiteMode(options.CookieSameSite), "Refresh token SameSite mode is invalid.")
            .ValidateOnStart();

        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SeedDataOptions>()
            .Bind(configuration.GetSection(SeedDataOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => !options.BootstrapAdmin.Enabled
                    || (!string.IsNullOrWhiteSpace(options.BootstrapAdmin.Email)
                        && !string.IsNullOrWhiteSpace(options.BootstrapAdmin.Password)),
                "Bootstrap admin email and password are required when bootstrap admin seeding is enabled.")
            .ValidateOnStart();
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DefaultConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string {DefaultConnectionName} is required.");
        }

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.CommandTimeout(databaseOptions.CommandTimeoutSeconds);
                    sqlOptions.EnableRetryOnFailure(
                        databaseOptions.MaxRetryCount,
                        TimeSpan.FromSeconds(databaseOptions.MaxRetryDelaySeconds),
                        errorNumbersToAdd: null);
                });
        });

        services.AddScoped<IApplicationDbContext>(serviceProvider =>
            serviceProvider.GetRequiredService<ApplicationDbContext>());
    }

    private static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                configuration.GetSection(IdentitySectionName).Bind(options);
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
    }

    private static void AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
        services.AddSingleton<IJwtTokenFactory, JwtTokenFactory>();
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
    }

    private static void AddAuthorizationServices(this IServiceCollection services)
    {
        var authorizationBuilder = services.AddAuthorizationBuilder()
            .AddPolicy(ApplicationPolicies.AuthenticatedUser, policy =>
            {
                policy.RequireAuthenticatedUser();
            })
            .AddPolicy(ApplicationPolicies.SystemAdministrator, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(ApplicationRoles.SystemAdministrator);
            });

        foreach (var permission in ApplicationPermissions.All)
        {
            authorizationBuilder.AddPolicy(permission.Code, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(permission.Code));
            });
        }
    }

    private static bool IsSupportedSameSiteMode(string value)
    {
        return value.Equals("Strict", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Lax", StringComparison.OrdinalIgnoreCase)
            || value.Equals("None", StringComparison.OrdinalIgnoreCase);
    }
}
