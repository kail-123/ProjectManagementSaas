using ProjectManagementSaaS.Application.Abstractions.Notifications;

namespace ProjectManagementSaaS.Api.Notifications;

internal sealed partial class NotificationBackgroundWorker(
    InMemoryNotificationDispatcher queue,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<NotificationBackgroundWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var intent in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<INotificationProcessor>();
                await processor.ProcessAsync(intent, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                NotificationProcessingFailed(logger, exception, intent.NotificationType.ToString());
            }
        }
    }

    [LoggerMessage(EventId = 51001, Level = LogLevel.Error, Message = "Failed to process notification intent {NotificationType}.")]
    private static partial void NotificationProcessingFailed(ILogger logger, Exception exception, string notificationType);
}
