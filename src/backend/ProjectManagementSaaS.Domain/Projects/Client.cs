using ProjectManagementSaaS.Domain.Common;
using ProjectManagementSaaS.Domain.Organizations;

namespace ProjectManagementSaaS.Domain.Projects;

public sealed class Client : SoftDeletableAuditableEntity
{
    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? GstNumber { get; set; }

    public string? Pan { get; set; }

    public string? BillingAddress { get; set; }

    public string? ShippingAddress { get; set; }

    public string Country { get; set; } = string.Empty;

    public string? State { get; set; }

    public string? City { get; set; }

    public string? Website { get; set; }

    public string? Notes { get; set; }

    public ClientStatus Status { get; set; } = ClientStatus.Active;

    public Organization Organization { get; set; } = null!;
}
