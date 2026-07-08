namespace ProjectManagementSaaS.Application.Features.Organizations.Contracts;

public sealed record TeamDto(
    Guid Id,
    Guid DepartmentId,
    string DepartmentName,
    Guid? TeamLeadUserProfileId,
    string? TeamLeadName,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<Guid> MemberUserProfileIds,
    string? CreatedBy,
    DateTimeOffset CreatedOn,
    string? UpdatedBy,
    DateTimeOffset? UpdatedOn);
