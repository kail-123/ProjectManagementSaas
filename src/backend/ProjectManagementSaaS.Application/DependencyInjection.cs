using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementSaaS.Application.Abstractions.Notifications;
using ProjectManagementSaaS.Application.Abstractions.Projects;
using ProjectManagementSaaS.Application.Features.Notifications;
using ProjectManagementSaaS.Application.Features.Projects;

namespace ProjectManagementSaaS.Application;

public static class DependencyInjection
{
    private const string AutoMapperLicenseKeyEnvironmentVariable = "AutoMapper__LicenseKey";

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly, includeInternalTypes: true);
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
        });
        services.AddAutoMapper(configuration =>
        {
            var licenseKey = Environment.GetEnvironmentVariable(AutoMapperLicenseKeyEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                configuration.LicenseKey = licenseKey;
            }

            configuration.AddMaps(typeof(ApplicationAssemblyMarker).Assembly);
        });
        services.AddScoped<IProjectAccessService, ProjectAccessService>();
        services.AddScoped<INotificationProcessor, NotificationProcessor>();

        return services;
    }
}
