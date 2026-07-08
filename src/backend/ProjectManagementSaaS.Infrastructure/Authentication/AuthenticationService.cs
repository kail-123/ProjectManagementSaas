using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectManagementSaaS.Application.Abstractions.Authentication;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Abstractions.Identity;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Features.Authentication.Contracts;
using ProjectManagementSaaS.Infrastructure.Persistence;
using ProjectManagementSaaS.Infrastructure.Persistence.Entities;

namespace ProjectManagementSaaS.Infrastructure.Authentication;

internal sealed partial class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext dbContext,
    IJwtTokenFactory jwtTokenFactory,
    ISecureTokenGenerator secureTokenGenerator,
    IUserPermissionService userPermissionService,
    IDateTimeProvider dateTimeProvider,
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    ILogger<AuthenticationService> logger)
    : IAuthenticationService
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";
    private const string RefreshTokenRotatedReason = "Refresh token rotated.";
    private const string RefreshTokenRevokedReason = "Refresh token revoked.";
    private const string RefreshTokenReuseReason = "Refresh token reuse detected.";
    private const string ActiveTokenLimitReason = "Active refresh token limit exceeded.";

    public async Task<AuthenticationSession> SignInAsync(
        SignInRequest request,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            LogUnknownEmailSignInAttempt(logger, secureTokenGenerator.HashToken(email));
            throw new ApplicationUnauthorizedException(InvalidCredentialsMessage);
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            LogFailedSignInAttempt(logger, user.Id, result.IsLockedOut);
            throw new ApplicationUnauthorizedException(InvalidCredentialsMessage);
        }

        if (!user.IsActive)
        {
            LogInactiveUserSignInAttempt(logger, user.Id);
            throw new ApplicationUnauthorizedException(InvalidCredentialsMessage);
        }

        user.LastLoginAtUtc = dateTimeProvider.UtcNow;

        return await CreateSessionAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthenticationSession> RefreshSessionAsync(
        RefreshTokenRequest request,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        var rawToken = EnsureRefreshToken(request.RefreshToken);
        var tokenHash = secureTokenGenerator.HashToken(rawToken);
        var storedToken = await dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
        {
            LogUnknownRefreshTokenAttempt(logger, tokenHash);
            throw new ApplicationUnauthorizedException("Refresh token is invalid.");
        }

        if (storedToken.RevokedAtUtc is not null)
        {
            await RevokeActiveRefreshTokensAsync(storedToken.UserId, ipAddress, RefreshTokenReuseReason, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            LogRefreshTokenReuseDetected(logger, storedToken.UserId);
            throw new ApplicationUnauthorizedException("Refresh token is invalid.");
        }

        if (!storedToken.IsActive(dateTimeProvider.UtcNow) || !storedToken.User.IsActive)
        {
            throw new ApplicationUnauthorizedException("Refresh token is invalid.");
        }

        var replacement = await CreateRefreshTokenAsync(storedToken.UserId, ipAddress, cancellationToken);
        storedToken.Revoke(dateTimeProvider.UtcNow, ipAddress, secureTokenGenerator.HashToken(replacement.Token), RefreshTokenRotatedReason);

        await dbContext.SaveChangesAsync(cancellationToken);

        return await CreateAuthenticationSessionAsync(storedToken.User, replacement, cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(
        RevokeRefreshTokenRequest request,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        var rawToken = EnsureRefreshToken(request.RefreshToken);
        var tokenHash = secureTokenGenerator.HashToken(rawToken);
        var storedToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || storedToken.RevokedAtUtc is not null)
        {
            return;
        }

        storedToken.Revoke(dateTimeProvider.UtcNow, ipAddress, replacementTokenHash: null, RefreshTokenRevokedReason);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
        {
            throw new ApplicationNotFoundException("User was not found.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var permissions = await userPermissionService.GetPermissionCodesAsync(user.Id, cancellationToken);

        return new CurrentUserResponse(user.Id, user.Email ?? string.Empty, user.DisplayName, roles.ToArray(), permissions);
    }

    private async Task<AuthenticationSession> CreateSessionAsync(
        ApplicationUser user,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        var refreshToken = await CreateRefreshTokenAsync(user.Id, ipAddress, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return await CreateAuthenticationSessionAsync(user, refreshToken, cancellationToken);
    }

    private async Task<(string Token, DateTimeOffset ExpiresAtUtc)> CreateRefreshTokenAsync(
        Guid userId,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        var options = refreshTokenOptions.Value;
        var now = dateTimeProvider.UtcNow;
        var rawToken = secureTokenGenerator.GenerateRefreshToken();
        var tokenHash = secureTokenGenerator.HashToken(rawToken);
        var expiresAtUtc = now.AddDays(options.ExpirationDays);

        var activeTokens = await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null && token.ExpiresAtUtc > now)
            .OrderBy(token => token.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens.Take(Math.Max(0, activeTokens.Count - options.MaxActiveTokensPerUser + 1)))
        {
            token.Revoke(now, ipAddress, replacementTokenHash: null, ActiveTokenLimitReason);
        }

        dbContext.RefreshTokens.Add(RefreshToken.Create(userId, tokenHash, expiresAtUtc, now, ipAddress));

        return (rawToken, expiresAtUtc);
    }

    private async Task<AuthenticationSession> CreateAuthenticationSessionAsync(
        ApplicationUser user,
        (string Token, DateTimeOffset ExpiresAtUtc) refreshToken,
        CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var permissions = await userPermissionService.GetPermissionCodesAsync(user.Id, cancellationToken);
        var accessToken = jwtTokenFactory.CreateToken(user, roles.ToArray(), permissions);
        var response = new AuthenticationResponse(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            new CurrentUserResponse(user.Id, user.Email ?? string.Empty, user.DisplayName, roles.ToArray(), permissions));

        return new AuthenticationSession(response, refreshToken.Token, refreshToken.ExpiresAtUtc);
    }

    private async Task RevokeActiveRefreshTokensAsync(
        Guid userId,
        string ipAddress,
        string reason,
        CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var activeTokens = await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null && token.ExpiresAtUtc > now)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(now, ipAddress, replacementTokenHash: null, reason);
        }
    }

    private static string EnsureRefreshToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ApplicationUnauthorizedException("Refresh token is required.");
        }

        return token;
    }

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Failed sign-in attempt for unknown email hash {EmailHash}.")]
    private static partial void LogUnknownEmailSignInAttempt(ILogger logger, string emailHash);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Failed sign-in attempt for user {UserId}. LockedOut: {LockedOut}.")]
    private static partial void LogFailedSignInAttempt(ILogger logger, Guid userId, bool lockedOut);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Inactive user {UserId} attempted to sign in.")]
    private static partial void LogInactiveUserSignInAttempt(ILogger logger, Guid userId);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Warning,
        Message = "Refresh attempt used an unknown refresh token hash {RefreshTokenHash}.")]
    private static partial void LogUnknownRefreshTokenAttempt(ILogger logger, string refreshTokenHash);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Warning,
        Message = "Refresh token reuse detected for user {UserId}.")]
    private static partial void LogRefreshTokenReuseDetected(ILogger logger, Guid userId);
}
