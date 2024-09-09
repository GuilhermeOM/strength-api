namespace Strength.Domain.Shared;

using Strength.Domain.Constants;

public interface IValidationResult
{
    public static readonly CustomError ValidationError = new(
        ErrorConstants.VALIDATIONERROR,
        "A validation problem occurred");

    CustomError[] Errors { get; }
}
