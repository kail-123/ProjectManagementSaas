namespace ProjectManagementSaaS.Application.Features.Users.Contracts;

public sealed record UserProfileDto(
    Guid Id,
    Guid UserId,
    Guid? OrganizationId,
    string? OrganizationName,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? TeamId,
    string? TeamName,
    string FirstName,
    string LastName,
    string EmployeeCode,
    string? Designation,
    string? ProfilePhoto,
    string? Phone,
    string TimeZone,
    string? Skills,
    DateOnly? JoiningDate,
    bool IsActive);
