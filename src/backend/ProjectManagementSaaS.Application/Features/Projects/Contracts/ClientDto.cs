using ProjectManagementSaaS.Domain.Projects;

namespace ProjectManagementSaaS.Application.Features.Projects.Contracts;

public sealed record ClientDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
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
    ClientStatus Status,
    string? CreatedBy,
    DateTimeOffset CreatedOn,
    string? UpdatedBy,
    DateTimeOffset? UpdatedOn);
