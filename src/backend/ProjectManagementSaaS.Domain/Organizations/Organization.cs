using ProjectManagementSaaS.Domain.Common;

namespace ProjectManagementSaaS.Domain.Organizations;

public sealed class Organization : SoftDeletableAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Logo { get; set; }

    public string? Website { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string Country { get; set; } = string.Empty;

    public string TimeZone { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
