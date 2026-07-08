namespace ProjectManagementSaaS.Domain.Organizations;

public sealed class TeamMember
{
    public Guid TeamId { get; set; }

    public Guid UserProfileId { get; set; }

    public DateTimeOffset AddedOn { get; set; }

    public string? AddedBy { get; set; }

    public Team Team { get; set; } = null!;

    public UserProfile UserProfile { get; set; } = null!;
}
