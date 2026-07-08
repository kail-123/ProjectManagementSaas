using FluentValidation.Results;

namespace ProjectManagementSaaS.Application.Common.Exceptions;

public sealed class ApplicationValidationException : Exception
{
    public ApplicationValidationException()
        : base("One or more validation failures occurred.")
    {
        Errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
    }

    public ApplicationValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(failure => failure.PropertyName, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
