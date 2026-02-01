using MediatR;
using Microsoft.Extensions.Logging;

namespace OrderService.Application.Behaviors;

/// <summary>
/// Pipeline behavior for validation (placeholder for future validation logic)
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Validating {RequestName}", requestName);

        // Add FluentValidation logic here if needed
        // For example:
        // var validators = _serviceProvider.GetServices<IValidator<TRequest>>();
        // var context = new ValidationContext<TRequest>(request);
        // var failures = validators
        //     .Select(v => v.Validate(context))
        //     .SelectMany(result => result.Errors)
        //     .Where(f => f != null)
        //     .ToList();
        // if (failures.Any())
        // {
        //     throw new ValidationException(failures);
        // }

        return await next();
    }
}
