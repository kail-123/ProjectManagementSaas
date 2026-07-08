using FluentValidation;
using ProjectManagementSaaS.Application.Features.Projects.Contracts;

namespace ProjectManagementSaaS.Application.Features.Projects.Validation;

public sealed class ProjectMemberUpsertRequestValidator : AbstractValidator<ProjectMemberUpsertRequest>
{
    public ProjectMemberUpsertRequestValidator()
    {
        RuleFor(request => request.UserIds).NotEmpty();
        RuleForEach(request => request.UserIds).NotEmpty();
        RuleFor(request => request.UserIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Project members must be unique.");
        RuleFor(request => request.RoleInProject).IsInEnum();
    }
}
