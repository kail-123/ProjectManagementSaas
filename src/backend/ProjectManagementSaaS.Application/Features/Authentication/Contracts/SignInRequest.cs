namespace ProjectManagementSaaS.Application.Features.Authentication.Contracts;

public sealed record SignInRequest(string Email, string Password);
