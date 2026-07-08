namespace ProjectManagementSaaS.Domain.Common;

public abstract class SoftDeletableAuditableEntity : IAuditableEntity, ISoftDeletable
{
    public Guid Id { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedOn { get; set; }

    public string? DeletedBy { get; set; }
}
