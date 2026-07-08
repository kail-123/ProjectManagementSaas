using FluentValidation;

namespace ProjectManagementSaaS.Application.Common.Models;

public sealed class BulkDeleteRequestValidator : AbstractValidator<BulkDeleteRequest>
{
    public BulkDeleteRequestValidator()
    {
        RuleFor(request => request.Ids)
            .NotEmpty()
            .Must(ids => ids.Count <= 500)
            .WithMessage("Bulk delete supports a maximum of 500 records per request.");
    }
}
