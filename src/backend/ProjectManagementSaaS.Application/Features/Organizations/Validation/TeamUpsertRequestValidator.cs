using FluentValidation;
using ProjectManagementSaaS.Application.Features.Organizations.Contracts;

namespace ProjectManagementSaaS.Application.Features.Organizations.Validation;

public sealed class TeamUpsertRequestValidator : AbstractValidator<TeamUpsertRequest>
{
    public TeamUpsertRequestValidator()
    {
        RuleFor(request => request.DepartmentId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(256);
        RuleFor(request => request.Code).NotEmpty().MaximumLength(64).Matches("^[A-Za-z0-9_-]+$");
        RuleFor(request => request.Description).MaximumLength(1024);
        RuleFor(request => request.MemberUserProfileIds).Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Team members must be unique.");
    }
}
