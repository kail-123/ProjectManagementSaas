using FluentValidation;
using ProjectManagementSaaS.Application.Features.Organizations.Contracts;

namespace ProjectManagementSaaS.Application.Features.Organizations.Validation;

public sealed class OrganizationUpsertRequestValidator : AbstractValidator<OrganizationUpsertRequest>
{
    public OrganizationUpsertRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(256);
        RuleFor(request => request.Code).NotEmpty().MaximumLength(64).Matches("^[A-Za-z0-9_-]+$");
        RuleFor(request => request.Logo).MaximumLength(1024);
        RuleFor(request => request.Website).MaximumLength(512).Must(BeValidOptionalUri).WithMessage("Website must be a valid URL.");
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Phone).MaximumLength(64);
        RuleFor(request => request.Address).MaximumLength(512);
        RuleFor(request => request.City).MaximumLength(128);
        RuleFor(request => request.State).MaximumLength(128);
        RuleFor(request => request.Country).NotEmpty().MaximumLength(128);
        RuleFor(request => request.TimeZone).NotEmpty().MaximumLength(128);
        RuleFor(request => request.Currency).NotEmpty().Length(3);
    }

    private static bool BeValidOptionalUri(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            || Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
