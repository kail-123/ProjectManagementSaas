using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Features.Security.Contracts;

namespace ProjectManagementSaaS.Application.Abstractions.Identity;

public interface IRoleManagementService
{
    Task<PagedResult<RoleDto>> ListAsync(PagedRequest request, CancellationToken cancellationToken);

    Task<RoleDto> GetByIdAsync(Guid roleId, CancellationToken cancellationToken);

    Task<RoleDto> CreateAsync(RoleUpsertRequest request, CancellationToken cancellationToken);

    Task<RoleDto> UpdateAsync(Guid roleId, RoleUpsertRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid roleId, CancellationToken cancellationToken);

    Task BulkDeleteAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken);

    Task AssignPermissionsAsync(Guid roleId, IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken);
}
