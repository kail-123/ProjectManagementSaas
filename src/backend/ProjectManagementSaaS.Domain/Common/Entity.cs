namespace ProjectManagementSaaS.Domain.Common;

public abstract class Entity<TKey>
    where TKey : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    public TKey Id { get; protected set; } = default!;

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
