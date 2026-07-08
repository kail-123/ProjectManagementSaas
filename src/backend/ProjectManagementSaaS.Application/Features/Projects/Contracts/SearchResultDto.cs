namespace ProjectManagementSaaS.Application.Features.Projects.Contracts;

public sealed record SearchResultDto(
    Guid Id,
    string Type,
    string Title,
    string? Subtitle,
    string Url);
