namespace ProjectManagementSaaS.Application.Abstractions.Common;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? UserName { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsInRole(string roleName);
}
