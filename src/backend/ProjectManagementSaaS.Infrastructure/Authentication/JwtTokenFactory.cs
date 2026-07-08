using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Infrastructure.Persistence.Entities;

namespace ProjectManagementSaaS.Infrastructure.Authentication;

internal sealed class JwtTokenFactory(
    IOptions<JwtOptions> jwtOptions,
    IDateTimeProvider dateTimeProvider)
    : IJwtTokenFactory
{
    public JwtToken CreateToken(
        ApplicationUser user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        var options = jwtOptions.Value;
        var expiresAtUtc = dateTimeProvider.UtcNow.AddMinutes(options.AccessTokenMinutes);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.DisplayName ?? user.Email ?? string.Empty),
            new(ApplicationClaimTypes.UserId, user.Id.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(permission => new Claim(ApplicationClaimTypes.Permission, permission)));

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: dateTimeProvider.UtcNow.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: signingCredentials);

        return new JwtToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
