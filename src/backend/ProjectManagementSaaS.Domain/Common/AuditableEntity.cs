namespace ProjectManagementSaaS.Domain.Common;

public abstract class AuditableEntity<TKey> : Entity<TKey>
    where TKey : notnull
{
    public DateTimeOffset CreatedAtUtc { get; protected set; }

    public string? CreatedBy { get; protected set; }

    public DateTimeOffset? LastModifiedAtUtc { get; protected set; }

    public string? LastModifiedBy { get; protected set; }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, string? createdBy)
    {
        CreatedAtUtc = createdAtUtc;
        CreatedBy = createdBy;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, string? modifiedBy)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedBy = modifiedBy;
    }
}
