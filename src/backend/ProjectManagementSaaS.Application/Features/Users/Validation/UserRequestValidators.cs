using FluentValidation;
using ProjectManagementSaaS.Application.Features.Users.Contracts;

namespace ProjectManagementSaaS.Application.Features.Users.Validation;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Password).NotEmpty().MinimumLength(12).MaximumLength(256);
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.EmployeeCode).NotEmpty().MaximumLength(64).Matches("^[A-Za-z0-9_-]+$");
        RuleFor(request => request.Designation).MaximumLength(256);
        RuleFor(request => request.ProfilePhoto).MaximumLength(1024);
        RuleFor(request => request.Phone).MaximumLength(64);
        RuleFor(request => request.TimeZone).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Skills).MaximumLength(2048);
        RuleForEach(request => request.Roles).MaximumLength(256);
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(128);
        RuleFor(request => request.EmployeeCode).NotEmpty().MaximumLength(64).Matches("^[A-Za-z0-9_-]+$");
        RuleFor(request => request.Designation).MaximumLength(256);
        RuleFor(request => request.ProfilePhoto).MaximumLength(1024);
        RuleFor(request => request.Phone).MaximumLength(64);
        RuleFor(request => request.TimeZone).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Skills).MaximumLength(2048);
        RuleForEach(request => request.Roles).MaximumLength(256);
    }
}

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(request => request.NewPassword).NotEmpty().MinimumLength(12).MaximumLength(256);
    }
}

public sealed class AssignRolesRequestValidator : AbstractValidator<AssignRolesRequest>
{
    public AssignRolesRequestValidator()
    {
        RuleFor(request => request.Roles).NotNull();
        RuleForEach(request => request.Roles).NotEmpty().MaximumLength(256);
    }
}
