namespace ProjectManagementSaaS.Domain.Audit;

public sealed class AuditLog
{
    public Guid Id { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? Changes { get; set; }

    public string? ChangedBy { get; set; }

    public DateTimeOffset ChangedOn { get; set; }
}
