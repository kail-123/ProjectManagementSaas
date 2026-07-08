using Microsoft.AspNetCore.Identity;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? LastLoginAtUtc { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();
}
