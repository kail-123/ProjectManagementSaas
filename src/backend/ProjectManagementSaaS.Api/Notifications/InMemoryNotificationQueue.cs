using System.Threading.Channels;
using ProjectManagementSaaS.Application.Abstractions.Notifications;

namespace ProjectManagementSaaS.Api.Notifications;

internal sealed class InMemoryNotificationDispatcher : INotificationDispatcher
{
    private readonly Channel<NotificationIntent> _channel = Channel.CreateUnbounded<NotificationIntent>(
        new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });

    public ValueTask EnqueueAsync(NotificationIntent intent, CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(intent, cancellationToken);
    }

    public IAsyncEnumerable<NotificationIntent> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
