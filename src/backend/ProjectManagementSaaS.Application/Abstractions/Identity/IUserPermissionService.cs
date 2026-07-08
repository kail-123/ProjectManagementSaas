namespace ProjectManagementSaaS.Application.Abstractions.Identity;

public interface IUserPermissionService
{
    Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(Guid userId, CancellationToken cancellationToken);

    void InvalidateUser(Guid userId);

    void InvalidateAll();
}
