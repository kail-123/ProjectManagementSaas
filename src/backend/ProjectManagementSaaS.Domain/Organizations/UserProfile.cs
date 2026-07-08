using ProjectManagementSaaS.Domain.Common;

namespace ProjectManagementSaaS.Domain.Organizations;

public sealed class UserProfile : SoftDeletableAuditableEntity
{
    public Guid UserId { get; set; }

    public Guid? OrganizationId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? TeamId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string EmployeeCode { get; set; } = string.Empty;

    public string? Designation { get; set; }

    public string? ProfilePhoto { get; set; }

    public string? Phone { get; set; }

    public string TimeZone { get; set; } = string.Empty;

    public string? Skills { get; set; }

    public DateOnly? JoiningDate { get; set; }

    public bool IsActive { get; set; } = true;

    public Organization? Organization { get; set; }

    public Department? Department { get; set; }

    public Team? Team { get; set; }
}
