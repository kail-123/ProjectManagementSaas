using FluentValidation;
using ProjectManagementSaaS.Application.Features.Projects.Contracts;

namespace ProjectManagementSaaS.Application.Features.Projects.Validation;

public sealed class ProjectSettingsRequestValidator : AbstractValidator<ProjectSettingsRequest>
{
    public ProjectSettingsRequestValidator()
    {
    }
}
