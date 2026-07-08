using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Abstractions.Identity;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Features.Security.Contracts;
using ProjectManagementSaaS.Domain.Security;
using ProjectManagementSaaS.Infrastructure.Persistence;
using ProjectManagementSaaS.Infrastructure.Persistence.Entities;

namespace ProjectManagementSaaS.Infrastructure.Identity;

internal sealed class RoleManagementService(
    RoleManager<ApplicationRole> roleManager,
    ApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUserPermissionService userPermissionService)
    : IRoleManagementService
{
    public async Task<PagedResult<RoleDto>> ListAsync(PagedRequest request, CancellationToken cancellationToken)
    {
        var query = dbContext.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(role =>
                role.Name != null && role.Name.Contains(term)
                || role.Description != null && role.Description.Contains(term));
        }

        var descending = request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        query = (request.SortBy?.ToLowerInvariant(), descending) switch
        {
            ("description", true) => query.OrderByDescending(role => role.Description),
            ("description", false) => query.OrderBy(role => role.Description),
            ("name", true) => query.OrderByDescending(role => role.Name),
            _ => query.OrderBy(role => role.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var roles = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RoleDto>(
            roles.Select(MapRole).ToArray(),
            request.PageNumber,
            request.PageSize,
            totalCount);
    }

    public async Task<RoleDto> GetByIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .Include(entity => entity.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == roleId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Role was not found.");

        return MapRole(role);
    }

    public async Task<RoleDto> CreateAsync(RoleUpsertRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description,
            CreatedAtUtc = dateTimeProvider.UtcNow
        };

        var result = await roleManager.CreateAsync(role);
        ThrowIfFailed(result, "Unable to create role.");
        userPermissionService.InvalidateAll();

        return await GetByIdAsync(role.Id, cancellationToken);
    }

    public async Task<RoleDto> UpdateAsync(Guid roleId, RoleUpsertRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new ApplicationNotFoundException("Role was not found.");

        role.Name = request.Name.Trim();
        role.Description = request.Description;

        var result = await roleManager.UpdateAsync(role);
        ThrowIfFailed(result, "Unable to update role.");
        userPermissionService.InvalidateAll();

        return await GetByIdAsync(role.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new ApplicationNotFoundException("Role was not found.");

        if (ApplicationRoles.BuiltIn.Contains(role.Name ?? string.Empty, StringComparer.Ordinal))
        {
            throw new ApplicationConflictException("Built-in roles cannot be deleted.");
        }

        var result = await roleManager.DeleteAsync(role);
        ThrowIfFailed(result, "Unable to delete role.");
        userPermissionService.InvalidateAll();
    }

    public async Task BulkDeleteAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken)
    {
        foreach (var roleId in roleIds.Distinct())
        {
            await DeleteAsync(roleId, cancellationToken);
        }
    }

    public async Task AssignPermissionsAsync(Guid roleId, IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .Include(entity => entity.RolePermissions)
            .SingleOrDefaultAsync(entity => entity.Id == roleId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Role was not found.");

        var distinctPermissionIds = permissionIds.Distinct().ToArray();
        var existingPermissionIds = await dbContext.Permissions
            .Where(permission => distinctPermissionIds.Contains(permission.Id))
            .Select(permission => permission.Id)
            .ToListAsync(cancellationToken);

        if (existingPermissionIds.Count != distinctPermissionIds.Length)
        {
            throw new ApplicationValidationException();
        }

        role.RolePermissions.Clear();
        foreach (var permissionId in distinctPermissionIds)
        {
            role.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId,
                AssignedOn = dateTimeProvider.UtcNow,
                AssignedBy = currentUserService.UserName
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        userPermissionService.InvalidateAll();
    }

    private static RoleDto MapRole(ApplicationRole role)
    {
        var permissions = role.RolePermissions
            .Where(rolePermission => rolePermission.Permission is not null)
            .Select(rolePermission => new PermissionDto(
                rolePermission.Permission.Id,
                rolePermission.Permission.Module,
                rolePermission.Permission.Name,
                rolePermission.Permission.Code,
                rolePermission.Permission.Description,
                rolePermission.Permission.IsActive,
                rolePermission.Permission.CreatedBy,
                rolePermission.Permission.CreatedOn,
                rolePermission.Permission.UpdatedBy,
                rolePermission.Permission.UpdatedOn))
            .OrderBy(permission => permission.Module, StringComparer.Ordinal)
            .ThenBy(permission => permission.Name, StringComparer.Ordinal)
            .ToArray();

        return new RoleDto(role.Id, role.Name ?? string.Empty, role.Description, permissions);
    }

    private static void ThrowIfFailed(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
        throw new ApplicationConflictException($"{message} {errors}");
    }
}
