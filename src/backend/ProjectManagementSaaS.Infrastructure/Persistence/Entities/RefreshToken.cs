namespace ProjectManagementSaaS.Infrastructure.Persistence.Entities;

public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc,
        string createdByIp)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
        CreatedByIp = createdByIp;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public string CreatedByIp { get; private set; } = string.Empty;

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? RevokedByIp { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public string? RevocationReason { get; private set; }

    public ApplicationUser User { get; private set; } = null!;

    public bool IsActive(DateTimeOffset utcNow)
    {
        return RevokedAtUtc is null && ExpiresAtUtc > utcNow;
    }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset createdAtUtc,
        string createdByIp)
    {
        return new RefreshToken(Guid.NewGuid(), userId, tokenHash, expiresAtUtc, createdAtUtc, createdByIp);
    }

    public void Revoke(
        DateTimeOffset revokedAtUtc,
        string revokedByIp,
        string? replacementTokenHash,
        string? reason)
    {
        RevokedAtUtc = revokedAtUtc;
        RevokedByIp = revokedByIp;
        ReplacedByTokenHash = replacementTokenHash;
        RevocationReason = reason;
    }
}
