using System.Globalization;
using ProjectManagementSaaS.Api;
using ProjectManagementSaaS.Api.Extensions;
using ProjectManagementSaaS.Api.Hubs;
using ProjectManagementSaaS.Api.Middleware;
using ProjectManagementSaaS.Application;
using ProjectManagementSaaS.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName();
    });

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddApiServices(builder.Configuration);

    var app = builder.Build();

    await app.InitializeDatabaseAsync();

    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in app.DescribeApiVersions())
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
        });
    }

    app.UseHttpsRedirection();
    app.UseCors(ApiCorsOptions.PolicyName);
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHealthChecks();

    await app.RunAsync();
}
catch (Exception exception) when (exception.GetType().Name == "HostAbortedException")
{
    throw;
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly.");
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
