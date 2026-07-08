using FluentValidation;
using ProjectManagementSaaS.Application.Features.Security.Contracts;

namespace ProjectManagementSaaS.Application.Features.Security.Validation;

public sealed class PermissionUpsertRequestValidator : AbstractValidator<PermissionUpsertRequest>
{
    public PermissionUpsertRequestValidator()
    {
        RuleFor(request => request.Module).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Name).NotEmpty().MaximumLength(256);
        RuleFor(request => request.Code).NotEmpty().MaximumLength(128).Matches("^[A-Za-z0-9_.:-]+$");
        RuleFor(request => request.Description).MaximumLength(1024);
    }
}
