using FluentValidation;
using ProjectManagementSaaS.Application.Features.Security.Contracts;

namespace ProjectManagementSaaS.Application.Features.Security.Validation;

public sealed class AssignPermissionsRequestValidator : AbstractValidator<AssignPermissionsRequest>
{
    public AssignPermissionsRequestValidator()
    {
        RuleFor(request => request.PermissionIds).NotNull();
        RuleForEach(request => request.PermissionIds).NotEmpty();
    }
}
