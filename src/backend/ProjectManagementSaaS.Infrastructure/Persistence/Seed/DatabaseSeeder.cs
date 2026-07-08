using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Domain.Security;
using ProjectManagementSaaS.Domain.Work;
using ProjectManagementSaaS.Infrastructure.Persistence.Entities;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Seed;

internal sealed partial class DatabaseSeeder(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IOptions<SeedDataOptions> seedOptions,
    ILogger<DatabaseSeeder> logger)
    : IDatabaseSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var options = seedOptions.Value;

        await EnsureSecurityCatalogAsync(cancellationToken);

        if (options.BootstrapAdmin.Enabled)
        {
            await EnsureBootstrapAdminAsync(options.BootstrapAdmin, cancellationToken);
        }

        await EnsureWorkManagementDefaultsAsync(cancellationToken);
    }

    private async Task EnsureSecurityCatalogAsync(CancellationToken cancellationToken)
    {
        await EnsureRoleAsync(ApplicationRoles.Developer, "Developer self-service role.", cancellationToken);
        await EnsureRoleAsync(ApplicationRoles.ProjectManager, "Project delivery management role.", cancellationToken);
        await EnsureRoleAsync(ApplicationRoles.OrganizationAdmin, "Organization administration role.", cancellationToken);
        await EnsureRoleAsync(ApplicationRoles.SuperAdmin, "Platform tenant and subscription administration role.", cancellationToken);
        await EnsureRoleAsync(ApplicationRoles.SystemAdministrator, "Unrestricted system administrator role.", cancellationToken);

        await EnsurePermissionCatalogAsync(cancellationToken);
        await EnsureRolePermissionsAsync(ApplicationRoles.Developer, ApplicationPermissions.Developer, cancellationToken);
        await EnsureRolePermissionsAsync(ApplicationRoles.ProjectManager, ApplicationPermissions.ProjectManager, cancellationToken);
        await EnsureRolePermissionsAsync(ApplicationRoles.OrganizationAdmin, ApplicationPermissions.OrganizationAdmin, cancellationToken);
        await EnsureRolePermissionsAsync(ApplicationRoles.SuperAdmin, ApplicationPermissions.SuperAdmin, cancellationToken);
        await EnsureRolePermissionsAsync(ApplicationRoles.SystemAdministrator, ApplicationPermissions.AllCodes, cancellationToken);
    }

    private async Task EnsureRoleAsync(string roleName, string description, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingRole = await roleManager.FindByNameAsync(roleName);
        if (existingRole is not null)
        {
            if (existingRole.Description != description)
            {
                existingRole.Description = description;
                var updateResult = await roleManager.UpdateAsync(existingRole);
                ThrowIfFailed(updateResult, $"Unable to update role {roleName}.");
            }

            return;
        }

        var result = await roleManager.CreateAsync(new ApplicationRole
        {
            Name = roleName,
            Description = description,
            CreatedAtUtc = dateTimeProvider.UtcNow
        });

        ThrowIfFailed(result, $"Unable to seed role {roleName}.");
        LogSeededSecurityRole(logger, roleName);
    }

    private async Task EnsurePermissionCatalogAsync(CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var definitionsByCode = ApplicationPermissions.All.ToDictionary(permission => permission.Code, StringComparer.OrdinalIgnoreCase);
        var definitionCodes = definitionsByCode.Keys.ToArray();
        var existingPermissions = await dbContext.Permissions
            .IgnoreQueryFilters()
            .Where(permission => definitionCodes.Contains(permission.Code))
            .ToDictionaryAsync(permission => permission.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var definition in ApplicationPermissions.All)
        {
            if (existingPermissions.TryGetValue(definition.Code, out var permission))
            {
                permission.Module = definition.Module;
                permission.Name = definition.Name;
                permission.Description = definition.Description;
                permission.IsActive = true;
                permission.IsDeleted = false;
                permission.DeletedBy = null;
                permission.DeletedOn = null;
                continue;
            }

            await dbContext.Permissions.AddAsync(
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Module = definition.Module,
                    Name = definition.Name,
                    Code = definition.Code,
                    Description = definition.Description,
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedOn = now
                },
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureRolePermissionsAsync(
        string roleName,
        IReadOnlyCollection<string> permissionCodes,
        CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .Include(entity => entity.RolePermissions)
            .SingleAsync(entity => entity.Name == roleName, cancellationToken);

        var desiredPermissions = await dbContext.Permissions
            .Where(permission => permissionCodes.Contains(permission.Code))
            .Select(permission => new { permission.Id, permission.Code })
            .ToArrayAsync(cancellationToken);

        if (desiredPermissions.Length != permissionCodes.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            throw new InvalidOperationException($"Unable to seed role permissions for {roleName}: permission catalog is incomplete.");
        }

        var desiredPermissionIds = desiredPermissions.Select(permission => permission.Id).ToHashSet();
        var currentPermissionIds = role.RolePermissions.Select(rolePermission => rolePermission.PermissionId).ToHashSet();

        foreach (var rolePermission in role.RolePermissions.Where(rolePermission => !desiredPermissionIds.Contains(rolePermission.PermissionId)).ToArray())
        {
            role.RolePermissions.Remove(rolePermission);
        }

        foreach (var permissionId in desiredPermissionIds.Where(permissionId => !currentPermissionIds.Contains(permissionId)))
        {
            role.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId,
                AssignedBy = "system",
                AssignedOn = dateTimeProvider.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureBootstrapAdminAsync(
        BootstrapAdminOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email = options.Email?.Trim()
            ?? throw new InvalidOperationException("Bootstrap admin email is required.");

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = options.DisplayName,
                CreatedAtUtc = dateTimeProvider.UtcNow,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(
                user,
                options.Password ?? throw new InvalidOperationException("Bootstrap admin password is required."));

            ThrowIfFailed(createResult, "Unable to seed bootstrap administrator.");
            LogSeededBootstrapAdministrator(logger, user.Id);
        }

        if (!await userManager.IsInRoleAsync(user, ApplicationRoles.SystemAdministrator))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, ApplicationRoles.SystemAdministrator);
            ThrowIfFailed(addRoleResult, "Unable to assign bootstrap administrator role.");
        }
    }

    private static void ThrowIfFailed(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
        throw new InvalidOperationException($"{message} {errors}");
    }

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Seeded security role {RoleName}.")]
    private static partial void LogSeededSecurityRole(ILogger logger, string roleName);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "Seeded bootstrap administrator user {UserId}.")]
    private static partial void LogSeededBootstrapAdministrator(ILogger logger, Guid userId);

    private async Task EnsureWorkManagementDefaultsAsync(CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var workflowId = Guid.Parse("4f16633f-23fb-4fcf-9639-d5879aaf1000");

        var workflow = await dbContext.WorkWorkflows
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(entity => entity.Code == WorkSeedCodes.WorkflowCode, cancellationToken);

        if (workflow is null)
        {
            workflow = new WorkWorkflow
            {
                Id = workflowId,
                Name = "Default Workflow",
                Code = WorkSeedCodes.WorkflowCode,
                Description = "Default work item lifecycle.",
                IsDefault = true,
                IsActive = true,
                CreatedBy = "system",
                CreatedOn = now
            };

            await dbContext.WorkWorkflows.AddAsync(workflow, cancellationToken);
        }

        var statusesByCode = await dbContext.WorkWorkflowStatuses
            .IgnoreQueryFilters()
            .Where(status => status.WorkflowId == workflow.Id)
            .ToDictionaryAsync(status => status.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var statusSeed in WorkSeedData.Statuses)
        {
            if (statusesByCode.ContainsKey(statusSeed.Code))
            {
                continue;
            }

            var status = new WorkWorkflowStatus
            {
                Id = statusSeed.Id,
                WorkflowId = workflow.Id,
                Name = statusSeed.Name,
                Code = statusSeed.Code,
                Color = statusSeed.Color,
                SortOrder = statusSeed.SortOrder,
                IsInitial = statusSeed.IsInitial,
                IsTerminal = statusSeed.IsTerminal,
                IsActive = true,
                CreatedBy = "system",
                CreatedOn = now
            };

            statusesByCode[status.Code] = status;
            await dbContext.WorkWorkflowStatuses.AddAsync(status, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        statusesByCode = await dbContext.WorkWorkflowStatuses
            .IgnoreQueryFilters()
            .Where(status => status.WorkflowId == workflow.Id)
            .ToDictionaryAsync(status => status.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var existingTransitions = await dbContext.WorkWorkflowTransitions
            .IgnoreQueryFilters()
            .Where(transition => transition.WorkflowId == workflow.Id)
            .Select(transition => transition.FromStatusId.ToString() + ":" + transition.ToStatusId.ToString())
            .ToListAsync(cancellationToken);

        var existingTransitionSet = existingTransitions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var transitionSeed in WorkSeedData.Transitions)
        {
            var fromStatus = statusesByCode[transitionSeed.FromCode];
            var toStatus = statusesByCode[transitionSeed.ToCode];
            var transitionKey = fromStatus.Id.ToString() + ":" + toStatus.Id.ToString();
            if (existingTransitionSet.Contains(transitionKey))
            {
                continue;
            }

            await dbContext.WorkWorkflowTransitions.AddAsync(
                new WorkWorkflowTransition
                {
                    Id = transitionSeed.Id,
                    WorkflowId = workflow.Id,
                    FromStatusId = fromStatus.Id,
                    ToStatusId = toStatus.Id,
                    Name = transitionSeed.Name,
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedOn = now
                },
                cancellationToken);
        }

        var existingPriorities = await dbContext.WorkPriorities
            .IgnoreQueryFilters()
            .Select(priority => priority.Code)
            .ToListAsync(cancellationToken);
        var existingPrioritySet = existingPriorities.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var prioritySeed in WorkSeedData.Priorities.Where(seed => !existingPrioritySet.Contains(seed.Code)))
        {
            await dbContext.WorkPriorities.AddAsync(
                new WorkPriority
                {
                    Id = prioritySeed.Id,
                    Name = prioritySeed.Name,
                    Code = prioritySeed.Code,
                    Color = prioritySeed.Color,
                    SortOrder = prioritySeed.SortOrder,
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedOn = now
                },
                cancellationToken);
        }

        var existingTypes = await dbContext.WorkItemTypes
            .IgnoreQueryFilters()
            .Select(type => type.Code)
            .ToListAsync(cancellationToken);
        var existingTypeSet = existingTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var typeSeed in WorkSeedData.Types.Where(seed => !existingTypeSet.Contains(seed.Code)))
        {
            await dbContext.WorkItemTypes.AddAsync(
                new WorkItemType
                {
                    Id = typeSeed.Id,
                    WorkflowId = workflow.Id,
                    Name = typeSeed.Name,
                    Code = typeSeed.Code,
                    Icon = typeSeed.Icon,
                    Color = typeSeed.Color,
                    Description = typeSeed.Description,
                    IsActive = true,
                    CreatedBy = "system",
                    CreatedOn = now
                },
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        LogSeededWorkManagementDefaults(logger);
    }

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Information,
        Message = "Ensured work management default workflow, priorities, and work item types.")]
    private static partial void LogSeededWorkManagementDefaults(ILogger logger);
}

file static class WorkSeedCodes
{
    public const string WorkflowCode = "DEFAULT-WORKFLOW";
}

file static class WorkSeedData
{
    public static readonly WorkStatusSeed[] Statuses =
    [
        new(Guid.Parse("6d6a506c-69c1-4ab7-a0bf-a2a7c7d61001"), "Open", "OPEN", "#64748b", 10, true, false),
        new(Guid.Parse("6d6a506c-69c1-4ab7-a0bf-a2a7c7d61002"), "Ready", "READY", "#2563eb", 20, false, false),
        new(Guid.Parse("6d6a506c-69c1-4ab7-a0bf-a2a7c7d61003"), "In Progress", "IN-PROGRESS", "#4f46e5", 30, false, false),
        new(Guid.Parse("6d6a506c-69c1-4ab7-a0bf-a2a7c7d61004"), "Code Review", "CODE-REVIEW", "#7c3aed", 40, false, false),
        new(Guid.Parse("6d6a506c-69c1-4ab7-a0bf-a2a7c7d61005"), "Testing", "TESTING", "#0891b2", 50, false, false),
        new(Guid.Parse("6d6a506c-69c1-4ab7-a0bf-a2a7c7d61006"), "UAT", "UAT", "#0d9488", 60, false, false),
        new(Guid.Parse("6d6a506c-69c1-4ab7-a0bf-a2a7c7d61007"), "Done", "DONE", "#16a34a", 70, false, true),
        new(Guid.Parse("6d6a506c-69c1-4ab7-a0bf-a2a7c7d61008"), "Closed", "CLOSED", "#475569", 80, false, true),
        new(Guid.Parse("6d6a506c-69c1-4ab7-a0bf-a2a7c7d61009"), "Rejected", "REJECTED", "#dc2626", 90, false, true),
        new(Guid.Parse("6d6a506c-69c1-4ab7-a0bf-a2a7c7d61010"), "Reopened", "REOPENED", "#d97706", 100, false, false)
    ];

    public static readonly WorkTransitionSeed[] Transitions =
    [
        new(Guid.Parse("3f6cd40e-aad8-43f2-ac0f-79fd16222001"), "OPEN", "READY", "Open to Ready"),
        new(Guid.Parse("3f6cd40e-aad8-43f2-ac0f-79fd16222002"), "READY", "IN-PROGRESS", "Ready to In Progress"),
        new(Guid.Parse("3f6cd40e-aad8-43f2-ac0f-79fd16222003"), "IN-PROGRESS", "CODE-REVIEW", "In Progress to Code Review"),
        new(Guid.Parse("3f6cd40e-aad8-43f2-ac0f-79fd16222004"), "CODE-REVIEW", "TESTING", "Code Review to Testing"),
        new(Guid.Parse("3f6cd40e-aad8-43f2-ac0f-79fd16222005"), "TESTING", "UAT", "Testing to UAT"),
        new(Guid.Parse("3f6cd40e-aad8-43f2-ac0f-79fd16222006"), "UAT", "DONE", "UAT to Done"),
        new(Guid.Parse("3f6cd40e-aad8-43f2-ac0f-79fd16222007"), "DONE", "CLOSED", "Done to Closed"),
        new(Guid.Parse("3f6cd40e-aad8-43f2-ac0f-79fd16222008"), "CLOSED", "REOPENED", "Closed to Reopened"),
        new(Guid.Parse("3f6cd40e-aad8-43f2-ac0f-79fd16222009"), "REOPENED", "IN-PROGRESS", "Reopened to In Progress"),
        new(Guid.Parse("3f6cd40e-aad8-43f2-ac0f-79fd16222010"), "OPEN", "REJECTED", "Open to Rejected"),
        new(Guid.Parse("3f6cd40e-aad8-43f2-ac0f-79fd16222011"), "READY", "REJECTED", "Ready to Rejected"),
        new(Guid.Parse("3f6cd40e-aad8-43f2-ac0f-79fd16222012"), "IN-PROGRESS", "REJECTED", "In Progress to Rejected")
    ];

    public static readonly WorkPrioritySeed[] Priorities =
    [
        new(Guid.Parse("9db6c082-f453-49c8-8db7-72c0c56a3001"), "Lowest", "LOWEST", "#94a3b8", 10),
        new(Guid.Parse("9db6c082-f453-49c8-8db7-72c0c56a3002"), "Low", "LOW", "#22c55e", 20),
        new(Guid.Parse("9db6c082-f453-49c8-8db7-72c0c56a3003"), "Medium", "MEDIUM", "#2563eb", 30),
        new(Guid.Parse("9db6c082-f453-49c8-8db7-72c0c56a3004"), "High", "HIGH", "#d97706", 40),
        new(Guid.Parse("9db6c082-f453-49c8-8db7-72c0c56a3005"), "Highest", "HIGHEST", "#dc2626", 50),
        new(Guid.Parse("9db6c082-f453-49c8-8db7-72c0c56a3006"), "Critical", "CRITICAL", "#7f1d1d", 60)
    ];

    public static readonly WorkTypeSeed[] Types =
    [
        new(Guid.Parse("36d95ce8-820f-4863-b79a-20d6a6974001"), "Epic", "EPIC", "layers", "#7c3aed", "Large outcome spanning multiple features or stories."),
        new(Guid.Parse("36d95ce8-820f-4863-b79a-20d6a6974002"), "Feature", "FEATURE", "package", "#2563eb", "Product capability delivered through one or more stories."),
        new(Guid.Parse("36d95ce8-820f-4863-b79a-20d6a6974003"), "Story", "STORY", "book-open", "#0891b2", "User-centered requirement with acceptance criteria."),
        new(Guid.Parse("36d95ce8-820f-4863-b79a-20d6a6974004"), "Task", "TASK", "check-square", "#475569", "Planned unit of work."),
        new(Guid.Parse("36d95ce8-820f-4863-b79a-20d6a6974005"), "Bug", "BUG", "bug", "#dc2626", "Defect or unexpected product behavior."),
        new(Guid.Parse("36d95ce8-820f-4863-b79a-20d6a6974006"), "Issue", "ISSUE", "alert-circle", "#d97706", "General issue requiring triage."),
        new(Guid.Parse("36d95ce8-820f-4863-b79a-20d6a6974007"), "Improvement", "IMPROVEMENT", "trending-up", "#16a34a", "Enhancement to existing behavior."),
        new(Guid.Parse("36d95ce8-820f-4863-b79a-20d6a6974008"), "Support", "SUPPORT", "life-buoy", "#0d9488", "Customer or internal support request."),
        new(Guid.Parse("36d95ce8-820f-4863-b79a-20d6a6974009"), "Sub Task", "SUB-TASK", "list-tree", "#64748b", "Child task under a parent work item.")
    ];
}

file sealed record WorkStatusSeed(Guid Id, string Name, string Code, string Color, int SortOrder, bool IsInitial, bool IsTerminal);

file sealed record WorkTransitionSeed(Guid Id, string FromCode, string ToCode, string Name);

file sealed record WorkPrioritySeed(Guid Id, string Name, string Code, string Color, int SortOrder);

file sealed record WorkTypeSeed(Guid Id, string Name, string Code, string Icon, string Color, string Description);
