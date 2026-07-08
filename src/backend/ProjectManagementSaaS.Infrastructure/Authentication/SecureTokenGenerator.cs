using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace ProjectManagementSaaS.Infrastructure.Authentication;

internal sealed class SecureTokenGenerator : ISecureTokenGenerator
{
    private const int RefreshTokenBytes = 64;

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(RefreshTokenBytes);

        return Base64UrlEncoder.Encode(bytes);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
}
