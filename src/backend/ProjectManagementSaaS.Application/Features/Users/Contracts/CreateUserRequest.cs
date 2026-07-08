namespace ProjectManagementSaaS.Application.Features.Users.Contracts;

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string EmployeeCode,
    string? Designation,
    Guid? OrganizationId,
    Guid? DepartmentId,
    Guid? TeamId,
    string? ProfilePhoto,
    string? Phone,
    string TimeZone,
    string? Skills,
    DateOnly? JoiningDate,
    bool IsActive,
    IReadOnlyCollection<string> Roles);
