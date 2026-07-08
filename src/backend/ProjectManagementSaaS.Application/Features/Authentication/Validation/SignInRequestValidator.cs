using FluentValidation;
using ProjectManagementSaaS.Application.Features.Authentication.Contracts;

namespace ProjectManagementSaaS.Application.Features.Authentication.Validation;

internal sealed class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MaximumLength(256);
    }
}
