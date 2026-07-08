using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Features.Users.Contracts;
using ProjectManagementSaaS.Application.Features.Work.Contracts;

namespace ProjectManagementSaaS.Application.Abstractions.Identity;

public interface IUserManagementService
{
    Task<PagedResult<UserDto>> ListAsync(PagedRequest request, CancellationToken cancellationToken);

    Task<UserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<PagedResult<WorkMentionCandidateDto>> ListProjectMentionCandidatesAsync(Guid projectId, PagedRequest request, CancellationToken cancellationToken);

    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);

    Task<UserDto> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken);

    Task SetActiveStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, CancellationToken cancellationToken);

    Task BulkDeleteAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken);

    Task ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken cancellationToken);

    Task AssignRolesAsync(Guid userId, IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken);
}
