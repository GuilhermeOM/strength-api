namespace Strength.Application.Abstractions.Behaviors;

using System.Net;
using Domain.Shared;
using FluentValidation;
using MediatR;

public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var asyncValidations = validators.Select(async validator => await validator.ValidateAsync(context));
        var asyncValidationsResult = await Task.WhenAll(asyncValidations);

        var errors = asyncValidationsResult
            .SelectMany(validationResult => validationResult.Errors)
            .Where(validationFailure => validationFailure is not null)
            .Select(failure => new CustomError(failure.PropertyName, failure.ErrorMessage))
            .Distinct()
            .ToArray();

        if (errors.Length > 0)
        {
            return CreateValidationResult<TResponse>(errors);
        }

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(CustomError[] errors)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (ResponseResult.WithErrors(HttpStatusCode.BadRequest, errors) as TResult)!;
        }

        var validationResult = typeof(ResponseResult)
            .GetMethod(nameof(ResponseResult.WithErrors), 1, [typeof(HttpStatusCode), typeof(CustomError[])])!
            .MakeGenericMethod(typeof(TResult).GenericTypeArguments[0])
            .Invoke(null, [HttpStatusCode.BadRequest, errors])!;

        return (TResult)validationResult;
    }
}
