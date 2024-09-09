namespace Strength.Domain.Shared;

public sealed class ValidationResult(CustomError[] errors)
    : Result(false, IValidationResult.ValidationError), IValidationResult
{
    public CustomError[] Errors { get; } = errors;

    public static ValidationResult WithErrors(CustomError[] errors) => new(errors);
}

public sealed class ValidationResult<TValue>(CustomError[] errors)
    : Result<TValue>(false, IValidationResult.ValidationError, default), IValidationResult
{
    public CustomError[] Errors => errors;

    public ValidationResult<TValue> WithErrors(CustomError[] errors) => new(errors);
}
