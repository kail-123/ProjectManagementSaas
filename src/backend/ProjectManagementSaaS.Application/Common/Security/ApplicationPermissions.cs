namespace ProjectManagementSaaS.Application.Common.Security;

public static class ApplicationPermissions
{
    public const string DashboardView = "dashboard.view";
    public const string MyTasksView = "my-tasks.view";
    public const string CalendarView = "calendar.view";
    public const string NotificationsView = "notifications.view";
    public const string ProfileView = "profile.view";
    public const string SearchGlobal = "search.global";

    public const string OrganizationsView = "organizations.view";
    public const string OrganizationsCreate = "organizations.create";
    public const string OrganizationsUpdate = "organizations.update";
    public const string OrganizationsDelete = "organizations.delete";
    public const string OrganizationsExport = "organizations.export";

    public const string DepartmentsView = "departments.view";
    public const string DepartmentsCreate = "departments.create";
    public const string DepartmentsUpdate = "departments.update";
    public const string DepartmentsDelete = "departments.delete";
    public const string DepartmentsExport = "departments.export";

    public const string TeamsView = "teams.view";
    public const string TeamsCreate = "teams.create";
    public const string TeamsUpdate = "teams.update";
    public const string TeamsDelete = "teams.delete";
    public const string TeamsExport = "teams.export";

    public const string UsersView = "users.view";
    public const string UsersCreate = "users.create";
    public const string UsersUpdate = "users.update";
    public const string UsersDelete = "users.delete";
    public const string UsersExport = "users.export";
    public const string UsersManageStatus = "users.manage-status";
    public const string UsersResetPassword = "users.reset-password";
    public const string UsersAssignRoles = "users.assign-roles";

    public const string RolesView = "roles.view";
    public const string RolesCreate = "roles.create";
    public const string RolesUpdate = "roles.update";
    public const string RolesDelete = "roles.delete";
    public const string RolesExport = "roles.export";
    public const string RolesAssignPermissions = "roles.assign-permissions";

    public const string PermissionsView = "permissions.view";
    public const string PermissionsCreate = "permissions.create";
    public const string PermissionsUpdate = "permissions.update";
    public const string PermissionsDelete = "permissions.delete";
    public const string PermissionsExport = "permissions.export";

    public const string ClientsView = "clients.view";
    public const string ClientsCreate = "clients.create";
    public const string ClientsUpdate = "clients.update";
    public const string ClientsDelete = "clients.delete";
    public const string ClientsExport = "clients.export";

    public const string ProjectsView = "projects.view";
    public const string ProjectsCreate = "projects.create";
    public const string ProjectsUpdate = "projects.update";
    public const string ProjectsDelete = "projects.delete";
    public const string ProjectsExport = "projects.export";
    public const string ProjectsDashboardView = "projects.dashboard.view";

    public const string ProjectMembersView = "project-members.view";
    public const string ProjectMembersManage = "project-members.manage";
    public const string ProjectMembersExport = "project-members.export";

    public const string ProjectSettingsView = "project-settings.view";
    public const string ProjectSettingsManage = "project-settings.manage";

    public const string WorkItemsView = "work-items.view";
    public const string WorkItemsCreate = "work-items.create";
    public const string WorkItemsUpdate = "work-items.update";
    public const string WorkItemsDelete = "work-items.delete";
    public const string WorkItemsExport = "work-items.export";
    public const string WorkItemsTransition = "work-items.transition";
    public const string WorkCommentsManage = "work-comments.manage";
    public const string WorkAttachmentsManage = "work-attachments.manage";
    public const string WorkLinksManage = "work-links.manage";
    public const string WorkFiltersManage = "work-filters.manage";

    public const string TenantsView = "tenants.view";
    public const string TenantsCreate = "tenants.create";
    public const string TenantsUpdate = "tenants.update";
    public const string TenantsDelete = "tenants.delete";
    public const string TenantsExport = "tenants.export";

    public const string SubscriptionsView = "subscriptions.view";
    public const string SubscriptionsCreate = "subscriptions.create";
    public const string SubscriptionsUpdate = "subscriptions.update";
    public const string SubscriptionsDelete = "subscriptions.delete";
    public const string SubscriptionsExport = "subscriptions.export";

    public static readonly IReadOnlyCollection<ApplicationPermissionDefinition> All =
    [
        new(DashboardView, "Workspace", "View Dashboard", "Access the workspace dashboard."),
        new(MyTasksView, "Workspace", "View My Tasks", "Access the signed-in user's task workspace."),
        new(CalendarView, "Workspace", "View Calendar", "Access calendar planning."),
        new(NotificationsView, "Workspace", "View Notifications", "Access notifications."),
        new(ProfileView, "Workspace", "View Profile", "Access the signed-in user's profile."),
        new(SearchGlobal, "Workspace", "Use Global Search", "Search across permitted workspace records."),

        new(OrganizationsView, "Organizations", "View Organizations", "View organization records."),
        new(OrganizationsCreate, "Organizations", "Create Organizations", "Create organization records."),
        new(OrganizationsUpdate, "Organizations", "Edit Organizations", "Update organization records."),
        new(OrganizationsDelete, "Organizations", "Delete Organizations", "Delete organization records."),
        new(OrganizationsExport, "Organizations", "Export Organizations", "Export organization records."),

        new(DepartmentsView, "Departments", "View Departments", "View department records."),
        new(DepartmentsCreate, "Departments", "Create Departments", "Create department records."),
        new(DepartmentsUpdate, "Departments", "Edit Departments", "Update department records."),
        new(DepartmentsDelete, "Departments", "Delete Departments", "Delete department records."),
        new(DepartmentsExport, "Departments", "Export Departments", "Export department records."),

        new(TeamsView, "Teams", "View Teams", "View team records."),
        new(TeamsCreate, "Teams", "Create Teams", "Create team records."),
        new(TeamsUpdate, "Teams", "Edit Teams", "Update team records."),
        new(TeamsDelete, "Teams", "Delete Teams", "Delete team records."),
        new(TeamsExport, "Teams", "Export Teams", "Export team records."),

        new(UsersView, "Users", "View Users", "View users and profiles."),
        new(UsersCreate, "Users", "Create Users", "Create user accounts."),
        new(UsersUpdate, "Users", "Edit Users", "Update user accounts."),
        new(UsersDelete, "Users", "Delete Users", "Delete user accounts."),
        new(UsersExport, "Users", "Export Users", "Export user records."),
        new(UsersManageStatus, "Users", "Manage User Status", "Activate or deactivate user accounts."),
        new(UsersResetPassword, "Users", "Reset User Passwords", "Reset user passwords."),
        new(UsersAssignRoles, "Users", "Assign User Roles", "Assign roles to users."),

        new(RolesView, "Roles", "View Roles", "View role records."),
        new(RolesCreate, "Roles", "Create Roles", "Create role records."),
        new(RolesUpdate, "Roles", "Edit Roles", "Update role records."),
        new(RolesDelete, "Roles", "Delete Roles", "Delete role records."),
        new(RolesExport, "Roles", "Export Roles", "Export role records."),
        new(RolesAssignPermissions, "Roles", "Assign Role Permissions", "Assign permissions to roles."),

        new(PermissionsView, "Permissions", "View Permissions", "View permission records."),
        new(PermissionsCreate, "Permissions", "Create Permissions", "Create permission records."),
        new(PermissionsUpdate, "Permissions", "Edit Permissions", "Update permission records."),
        new(PermissionsDelete, "Permissions", "Delete Permissions", "Delete permission records."),
        new(PermissionsExport, "Permissions", "Export Permissions", "Export permission records."),

        new(ClientsView, "Clients", "View Clients", "View client records."),
        new(ClientsCreate, "Clients", "Create Clients", "Create client records."),
        new(ClientsUpdate, "Clients", "Edit Clients", "Update client records."),
        new(ClientsDelete, "Clients", "Delete Clients", "Delete client records."),
        new(ClientsExport, "Clients", "Export Clients", "Export client records."),

        new(ProjectsView, "Projects", "View Projects", "View project records."),
        new(ProjectsCreate, "Projects", "Create Projects", "Create project records."),
        new(ProjectsUpdate, "Projects", "Edit Projects", "Update project records."),
        new(ProjectsDelete, "Projects", "Delete Projects", "Delete project records."),
        new(ProjectsExport, "Projects", "Export Projects", "Export project records."),
        new(ProjectsDashboardView, "Projects", "View Project Dashboards", "View project dashboards."),

        new(ProjectMembersView, "Project Members", "View Project Members", "View project members."),
        new(ProjectMembersManage, "Project Members", "Manage Project Members", "Add, update, or remove project members."),
        new(ProjectMembersExport, "Project Members", "Export Project Members", "Export project member records."),

        new(ProjectSettingsView, "Project Settings", "View Project Settings", "View project settings."),
        new(ProjectSettingsManage, "Project Settings", "Manage Project Settings", "Update project settings."),

        new(WorkItemsView, "Work Items", "View Work Items", "View work items and related details."),
        new(WorkItemsCreate, "Work Items", "Create Work Items", "Create work items."),
        new(WorkItemsUpdate, "Work Items", "Edit Work Items", "Update work items."),
        new(WorkItemsDelete, "Work Items", "Delete Work Items", "Delete work items."),
        new(WorkItemsExport, "Work Items", "Export Work Items", "Export work item records."),
        new(WorkItemsTransition, "Work Items", "Transition Work Items", "Move work items through workflow statuses."),
        new(WorkCommentsManage, "Work Items", "Manage Work Comments", "Add, update, or delete work item comments."),
        new(WorkAttachmentsManage, "Work Items", "Manage Work Attachments", "Add or delete work item attachments."),
        new(WorkLinksManage, "Work Items", "Manage Work Links", "Add or delete work item links."),
        new(WorkFiltersManage, "Work Items", "Manage Work Filters", "Create or delete work item saved filters."),

        new(TenantsView, "Tenants", "View Tenants", "View tenant records."),
        new(TenantsCreate, "Tenants", "Create Tenants", "Create tenant records."),
        new(TenantsUpdate, "Tenants", "Edit Tenants", "Update tenant records."),
        new(TenantsDelete, "Tenants", "Delete Tenants", "Delete tenant records."),
        new(TenantsExport, "Tenants", "Export Tenants", "Export tenant records."),

        new(SubscriptionsView, "Subscriptions", "View Subscriptions", "View subscription records."),
        new(SubscriptionsCreate, "Subscriptions", "Create Subscriptions", "Create subscription records."),
        new(SubscriptionsUpdate, "Subscriptions", "Edit Subscriptions", "Update subscription records."),
        new(SubscriptionsDelete, "Subscriptions", "Delete Subscriptions", "Delete subscription records."),
        new(SubscriptionsExport, "Subscriptions", "Export Subscriptions", "Export subscription records.")
    ];

    public static readonly IReadOnlyCollection<string> Developer =
    [
        DashboardView,
        MyTasksView,
        CalendarView,
        NotificationsView,
        ProfileView
    ];

    public static readonly IReadOnlyCollection<string> ProjectManager =
    [
        DashboardView,
        ProfileView,
        NotificationsView,
        CalendarView,
        SearchGlobal,
        ClientsView,
        ClientsCreate,
        ClientsUpdate,
        ClientsExport,
        ProjectsView,
        ProjectsCreate,
        ProjectsUpdate,
        ProjectsDelete,
        ProjectsExport,
        ProjectsDashboardView,
        ProjectMembersView,
        ProjectMembersManage,
        ProjectMembersExport,
        ProjectSettingsView,
        ProjectSettingsManage,
        WorkItemsView,
        WorkItemsCreate,
        WorkItemsUpdate,
        WorkItemsDelete,
        WorkItemsExport,
        WorkItemsTransition,
        WorkCommentsManage,
        WorkAttachmentsManage,
        WorkLinksManage,
        WorkFiltersManage
    ];

    public static readonly IReadOnlyCollection<string> OrganizationAdmin =
    [
        DashboardView,
        ProfileView,
        NotificationsView,
        CalendarView,
        SearchGlobal,
        OrganizationsView,
        OrganizationsUpdate,
        OrganizationsExport,
        DepartmentsView,
        DepartmentsCreate,
        DepartmentsUpdate,
        DepartmentsDelete,
        DepartmentsExport,
        TeamsView,
        TeamsCreate,
        TeamsUpdate,
        TeamsDelete,
        TeamsExport,
        UsersView,
        UsersCreate,
        UsersUpdate,
        UsersDelete,
        UsersExport,
        UsersManageStatus,
        UsersResetPassword,
        UsersAssignRoles,
        RolesView,
        RolesCreate,
        RolesUpdate,
        RolesDelete,
        RolesExport,
        RolesAssignPermissions,
        PermissionsView,
        ProjectsView,
        ProjectsCreate,
        ProjectsUpdate,
        ProjectsDelete,
        ProjectsExport,
        ProjectsDashboardView,
        ProjectMembersView,
        ProjectMembersManage,
        ProjectMembersExport,
        ProjectSettingsView,
        ProjectSettingsManage,
        WorkItemsView,
        WorkItemsCreate,
        WorkItemsUpdate,
        WorkItemsDelete,
        WorkItemsExport,
        WorkItemsTransition,
        WorkCommentsManage,
        WorkAttachmentsManage,
        WorkLinksManage,
        WorkFiltersManage
    ];

    public static readonly IReadOnlyCollection<string> SuperAdmin =
    [
        DashboardView,
        ProfileView,
        NotificationsView,
        TenantsView,
        TenantsCreate,
        TenantsUpdate,
        TenantsDelete,
        TenantsExport,
        SubscriptionsView,
        SubscriptionsCreate,
        SubscriptionsUpdate,
        SubscriptionsDelete,
        SubscriptionsExport,
        OrganizationsView,
        OrganizationsCreate,
        OrganizationsUpdate,
        OrganizationsDelete,
        OrganizationsExport
    ];

    public static IReadOnlyCollection<string> AllCodes => All.Select(permission => permission.Code).ToArray();
}

public sealed record ApplicationPermissionDefinition(
    string Code,
    string Module,
    string Name,
    string Description);
