namespace ProjectManagementSaaS.Application.Features.Projects.Contracts;

public sealed record ProjectSettingsDto(
    Guid ProjectId,
    bool EnableSprint,
    bool EnableBacklog,
    bool EnableKanban,
    bool EnableTimeTracking,
    bool EnableWiki,
    bool EnableDocuments,
    bool EnableApprovals,
    bool EnableLeaveRequests,
    bool EnableBugTracking,
    bool EnableRiskRegister,
    string? CreatedBy,
    DateTimeOffset CreatedOn,
    string? UpdatedBy,
    DateTimeOffset? UpdatedOn);
