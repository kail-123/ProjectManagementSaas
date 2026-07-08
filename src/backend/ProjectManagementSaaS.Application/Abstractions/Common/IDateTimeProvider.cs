namespace ProjectManagementSaaS.Application.Abstractions.Common;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
