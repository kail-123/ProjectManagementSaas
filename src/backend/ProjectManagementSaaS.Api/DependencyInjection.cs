using Asp.Versioning;
using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using ProjectManagementSaaS.Api.Notifications;
using ProjectManagementSaaS.Api.Services;
using ProjectManagementSaaS.Application.Abstractions.Notifications;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Api.Filters;
using ProjectManagementSaaS.Api.OpenApi;
using System.Text.Json.Serialization;

namespace ProjectManagementSaaS.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

        services.AddOptions<ApiCorsOptions>()
            .Bind(configuration.GetSection(ApiCorsOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => options.AllowedOrigins.All(IsValidOrigin), "CORS origins must be absolute HTTP or HTTPS URLs.")
            .ValidateOnStart();

        services.AddScoped<FluentValidationActionFilter>();
        services.AddControllers(options =>
        {
            options.Filters.AddService<FluentValidationActionFilter>();
        })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        services.AddSignalR()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddSingleton<InMemoryNotificationDispatcher>();
        services.AddSingleton<INotificationDispatcher>(serviceProvider => serviceProvider.GetRequiredService<InMemoryNotificationDispatcher>());
        services.AddHostedService<NotificationBackgroundWorker>();
        services.AddScoped<INotificationRealtimePublisher, SignalRNotificationRealtimePublisher>();

        services.AddValidatorsFromAssembly(typeof(ApiAssemblyMarker).Assembly, includeInternalTypes: true);

        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Provide a valid JWT access token."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        services.AddCors(options =>
        {
            var corsOptions = configuration.GetSection(ApiCorsOptions.SectionName).Get<ApiCorsOptions>() ?? new ApiCorsOptions();

            options.AddPolicy(ApiCorsOptions.PolicyName, policyBuilder =>
            {
                policyBuilder
                    .WithOrigins(corsOptions.AllowedOrigins)
                    .WithHeaders("Authorization", "Content-Type", "X-Requested-With")
                    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS");

                if (corsOptions.AllowCredentials)
                {
                    policyBuilder.AllowCredentials();
                }
            });
        });

        return services;
    }

    private static bool IsValidOrigin(string origin)
    {
        return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
