using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectManagementSaaS.Application.Abstractions.Authentication;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Features.Authentication.Contracts;
using ProjectManagementSaaS.Infrastructure.Authentication;

namespace ProjectManagementSaaS.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController(
    IAuthenticationService authenticationService,
    IOptions<RefreshTokenOptions> refreshTokenOptions)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("token")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> CreateToken(
        [FromBody] SignInRequest request,
        CancellationToken cancellationToken)
    {
        var session = await authenticationService.SignInAsync(request, GetRemoteIpAddress(), cancellationToken);
        AppendRefreshTokenCookie(session);

        return Ok(session.Response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Refresh(CancellationToken cancellationToken)
    {
        var session = await authenticationService.RefreshSessionAsync(
            new RefreshTokenRequest(ReadRefreshTokenCookie()),
            GetRemoteIpAddress(),
            cancellationToken);

        AppendRefreshTokenCookie(session);

        return Ok(session.Response);
    }

    [Authorize(Policy = ApplicationPolicies.AuthenticatedUser)]
    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Revoke(CancellationToken cancellationToken)
    {
        await authenticationService.RevokeRefreshTokenAsync(
            new RevokeRefreshTokenRequest(ReadRefreshTokenCookie()),
            GetRemoteIpAddress(),
            cancellationToken);

        DeleteRefreshTokenCookie();

        return NoContent();
    }

    [Authorize(Policy = ApplicationPolicies.AuthenticatedUser)]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ApplicationClaimTypes.UserId);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new ApplicationUnauthorizedException("Authenticated user id claim is missing or invalid.");
        }

        var response = await authenticationService.GetCurrentUserAsync(userId, cancellationToken);

        return Ok(response);
    }

    private void AppendRefreshTokenCookie(AuthenticationSession session)
    {
        Response.Cookies.Append(
            refreshTokenOptions.Value.CookieName,
            session.RefreshToken,
            BuildRefreshTokenCookieOptions(session.RefreshTokenExpiresAtUtc));
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(
            refreshTokenOptions.Value.CookieName,
            BuildRefreshTokenCookieOptions(DateTimeOffset.UnixEpoch));
    }

    private string? ReadRefreshTokenCookie()
    {
        return Request.Cookies.TryGetValue(refreshTokenOptions.Value.CookieName, out var token)
            ? token
            : null;
    }

    private CookieOptions BuildRefreshTokenCookieOptions(DateTimeOffset expiresAtUtc)
    {
        var options = refreshTokenOptions.Value;

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = options.CookieSecure,
            SameSite = Enum.Parse<SameSiteMode>(options.CookieSameSite, ignoreCase: true),
            Expires = expiresAtUtc,
            Path = "/"
        };
    }

    private string GetRemoteIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unavailable";
    }
}
