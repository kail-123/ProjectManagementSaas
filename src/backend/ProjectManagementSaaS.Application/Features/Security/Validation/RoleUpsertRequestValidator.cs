using FluentValidation;
using ProjectManagementSaaS.Application.Features.Security.Contracts;

namespace ProjectManagementSaaS.Application.Features.Security.Validation;

public sealed class RoleUpsertRequestValidator : AbstractValidator<RoleUpsertRequest>
{
    public RoleUpsertRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(256).Matches("^[A-Za-z0-9 _-]+$");
        RuleFor(request => request.Description).MaximumLength(512);
    }
}
