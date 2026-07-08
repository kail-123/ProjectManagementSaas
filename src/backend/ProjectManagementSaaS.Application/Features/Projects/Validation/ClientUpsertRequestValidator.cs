using FluentValidation;
using ProjectManagementSaaS.Application.Features.Projects.Contracts;

namespace ProjectManagementSaaS.Application.Features.Projects.Validation;

public sealed class ClientUpsertRequestValidator : AbstractValidator<ClientUpsertRequest>
{
    public ClientUpsertRequestValidator()
    {
        RuleFor(request => request.OrganizationId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(256);
        RuleFor(request => request.Code).NotEmpty().MaximumLength(64).Matches("^[A-Za-z0-9_-]+$");
        RuleFor(request => request.ContactPerson).MaximumLength(256);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Phone).MaximumLength(64);
        RuleFor(request => request.Mobile).MaximumLength(64);
        RuleFor(request => request.GstNumber).MaximumLength(32);
        RuleFor(request => request.Pan).MaximumLength(32);
        RuleFor(request => request.BillingAddress).MaximumLength(1024);
        RuleFor(request => request.ShippingAddress).MaximumLength(1024);
        RuleFor(request => request.Country).NotEmpty().MaximumLength(128);
        RuleFor(request => request.State).MaximumLength(128);
        RuleFor(request => request.City).MaximumLength(128);
        RuleFor(request => request.Website).MaximumLength(512).Must(BeValidOptionalUri).WithMessage("Website must be a valid URL.");
        RuleFor(request => request.Notes).MaximumLength(2048);
        RuleFor(request => request.Status).IsInEnum();
    }

    private static bool BeValidOptionalUri(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            || Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
