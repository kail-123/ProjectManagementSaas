using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Application.Features.Projects.Contracts;

public sealed record ClientUpsertRequest(
    Guid OrganizationId,
    string Name,
    string Code,
    string? ContactPerson,
    string Email,
    string? Phone,
    string? Mobile,
    string? GstNumber,
    string? Pan,
    string? BillingAddress,
    string? ShippingAddress,
    string Country,
    string? State,
    string? City,
    string? Website,
    string? Notes,
    ClientStatus Status);
