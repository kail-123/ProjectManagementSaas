using ProjectManagementSaaS.Application.Abstractions.Common;

namespace ProjectManagementSaaS.Infrastructure.Common;

internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
