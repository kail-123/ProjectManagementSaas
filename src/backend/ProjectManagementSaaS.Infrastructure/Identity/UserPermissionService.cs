using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Abstractions.Identity;
using ProjectManagementSaaS.Infrastructure.Persistence;

namespace ProjectManagementSaaS.Infrastructure.Identity;

internal sealed class UserPermissionService(
    ApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
    : IUserPermissionService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly ConcurrentDictionary<Guid, CachedPermissions> Cache = new();

    public async Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        if (Cache.TryGetValue(userId, out var cached) && cached.ExpiresAtUtc > now)
        {
            return cached.Codes;
        }

        var codes = await (
                from userRole in dbContext.UserRoles.AsNoTracking()
                join rolePermission in dbContext.RolePermissions.AsNoTracking()
                    on userRole.RoleId equals rolePermission.RoleId
                join permission in dbContext.Permissions.AsNoTracking()
                    on rolePermission.PermissionId equals permission.Id
                where userRole.UserId == userId && permission.IsActive
                select permission.Code)
            .Distinct()
            .OrderBy(code => code)
            .ToArrayAsync(cancellationToken);

        Cache[userId] = new CachedPermissions(codes, now.Add(CacheDuration));

        return codes;
    }

    public void InvalidateUser(Guid userId)
    {
        Cache.TryRemove(userId, out _);
    }

    public void InvalidateAll()
    {
        Cache.Clear();
    }

    private sealed record CachedPermissions(IReadOnlyCollection<string> Codes, DateTimeOffset ExpiresAtUtc);
}
