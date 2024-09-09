namespace Strength.Domain.Shared;

public class Result
{
    protected Result(bool isSuccess, CustomError error)
    {
        if ((isSuccess && error != CustomError.None) || (!isSuccess && error == CustomError.None))
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        this.IsSuccess = isSuccess;
        this.Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !this.IsSuccess;
    public CustomError Error { get; }

    public static Result Success() => new(true, CustomError.None);
    public static Result<TValue> Success<TValue>(TValue value) => new(true, CustomError.None, value);
    public static Result Failure(CustomError error) => new(false, error);
    public static Result<TValue> Failure<TValue>(CustomError error) => new(false, error, default);

}

public class Result<TValue> : Result
{
    private readonly TValue? value;

    protected internal Result(bool isSuccess, CustomError error, TValue? value) : base(isSuccess, error) => this.value = value;

    public TValue Value => this.IsSuccess ? this.value! : throw new InvalidOperationException("The value of a failure result can not be accessed.");
}
