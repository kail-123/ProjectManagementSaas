using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProjectManagementSaaS.Api.Filters;

public sealed class FluentValidationActionFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var failures = new List<ValidationFailure>();

        foreach (var argument in context.ActionArguments.Values.Where(value => value is not null))
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(argument!.GetType());

            if (serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            failures.AddRange(result.Errors.Where(error => error is not null));
        }

        if (failures.Count > 0)
        {
            var errors = failures
                .GroupBy(failure => failure.PropertyName, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).Distinct(StringComparer.Ordinal).ToArray(),
                    StringComparer.Ordinal);

            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed.",
                Type = "https://httpstatuses.com/400",
                Instance = context.HttpContext.Request.Path
            });

            return;
        }

        await next();
    }
}
