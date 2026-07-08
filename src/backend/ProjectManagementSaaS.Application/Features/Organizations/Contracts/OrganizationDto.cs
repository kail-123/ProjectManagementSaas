namespace ProjectManagementSaaS.Application.Features.Organizations.Contracts;

public sealed record OrganizationDto(
    Guid Id,
    string Name,
    string Code,
    string? Logo,
    string? Website,
    string Email,
    string? Phone,
    string? Address,
    string? City,
    string? State,
    string Country,
    string TimeZone,
    string Currency,
    bool IsActive,
    string? CreatedBy,
    DateTimeOffset CreatedOn,
    string? UpdatedBy,
    DateTimeOffset? UpdatedOn);
