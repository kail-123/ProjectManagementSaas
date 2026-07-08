using FluentValidation;

namespace ProjectManagementSaaS.Application.Common.Models;

public sealed class PagedRequestValidator : AbstractValidator<PagedRequest>
{
    public PagedRequestValidator()
    {
        RuleFor(request => request.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 200);
        RuleFor(request => request.Search).MaximumLength(256);
        RuleFor(request => request.SortBy).MaximumLength(128);
        RuleFor(request => request.SortDirection)
            .Must(direction => string.IsNullOrWhiteSpace(direction)
                || direction.Equals("asc", StringComparison.OrdinalIgnoreCase)
                || direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be asc or desc.");
    }
}
