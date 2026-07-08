using FluentValidation;
using ProjectManagementSaaS.Application.Features.Projects.Contracts;

namespace ProjectManagementSaaS.Application.Features.Projects.Validation;

public sealed class ProjectUpsertRequestValidator : AbstractValidator<ProjectUpsertRequest>
{
    public ProjectUpsertRequestValidator()
    {
        RuleFor(request => request.OrganizationId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(256);
        RuleFor(request => request.Code).NotEmpty().MaximumLength(64).Matches("^[A-Za-z0-9_-]+$");
        RuleFor(request => request.Description).MaximumLength(2048);
        RuleFor(request => request.EstimatedBudget).GreaterThanOrEqualTo(0).When(request => request.EstimatedBudget.HasValue);
        RuleFor(request => request.Status).IsInEnum();
        RuleFor(request => request.Priority).IsInEnum();
        RuleFor(request => request.Visibility).IsInEnum();
        RuleFor(request => request.TeamIds)
            .NotNull()
            .Must(ids => ids is not null && ids.Distinct().Count() == ids.Count)
            .WithMessage("Project teams must be unique.");
        RuleFor(request => request)
            .Must(request => !request.StartDate.HasValue || !request.EndDate.HasValue || request.EndDate.Value >= request.StartDate.Value)
            .WithMessage("End date must be on or after start date.");
    }
}
