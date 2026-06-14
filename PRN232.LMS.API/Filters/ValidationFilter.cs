using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PRN232.LMS.Services.ResponseModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN232.LMS.API.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new List<string>();

        // 1. Run FluentValidation
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator != null)
            {
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext);

                if (!validationResult.IsValid)
                {
                    var fluentErrors = validationResult.Errors
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    errors.AddRange(fluentErrors);
                }
            }
        }

        // 2. Add any DataAnnotations errors
        if (!context.ModelState.IsValid)
        {
            var modelStateErrors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            errors.AddRange(modelStateErrors);
        }

        // 3. Return combined response if any errors exist
        if (errors.Any())
        {
            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.Fail("Validation failed. Please check the submitted data.", errors));
            return;
        }

        await next();
    }
}
