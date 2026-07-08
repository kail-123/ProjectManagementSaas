namespace ProjectManagementSaaS.Application.Common.Security;

public static class ApplicationRoles
{
    public const string Developer = "Developer";

    public const string ProjectManager = "Project Manager";

    public const string OrganizationAdmin = "Organization Admin";

    public const string SuperAdmin = "Super Admin";

    public const string SystemAdministrator = nameof(SystemAdministrator);

    public static readonly IReadOnlyCollection<string> BuiltIn =
    [
        Developer,
        ProjectManager,
        OrganizationAdmin,
        SuperAdmin,
        SystemAdministrator
    ];
}
