namespace ProjectManagementSaaS.Infrastructure.Authentication;

internal interface ISecureTokenGenerator
{
    string GenerateRefreshToken();

    string HashToken(string token);
}
