namespace ProjectManagementSaaS.Domain.Common;

public interface IAuditableEntity
{
    string? CreatedBy { get; set; }

    DateTimeOffset CreatedOn { get; set; }

    string? UpdatedBy { get; set; }

    DateTimeOffset? UpdatedOn { get; set; }
}
