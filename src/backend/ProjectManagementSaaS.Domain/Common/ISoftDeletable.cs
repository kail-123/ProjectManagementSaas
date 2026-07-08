namespace ProjectManagementSaaS.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    DateTimeOffset? DeletedOn { get; set; }

    string? DeletedBy { get; set; }
}
