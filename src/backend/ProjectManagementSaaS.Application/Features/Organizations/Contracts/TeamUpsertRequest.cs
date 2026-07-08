namespace ProjectManagementSaaS.Application.Features.Organizations.Contracts;

public sealed record TeamUpsertRequest(
    Guid DepartmentId,
    Guid? TeamLeadUserProfileId,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<Guid> MemberUserProfileIds);
