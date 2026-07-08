namespace ProjectManagementSaaS.Domain.Common;

public abstract record DomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
