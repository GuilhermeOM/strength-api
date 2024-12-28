namespace Strength.Domain.Shared;

using System.Net;

public sealed class ResponseResult(HttpStatusCode statusCode, CustomError[] errors)
    : Result(false, IResponseResult.ResponseError), IResponseResult
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public CustomError[] Errors { get; } = errors;

    public static ResponseResult WithErrors(HttpStatusCode statusCode, CustomError[] errors) => new(statusCode, errors);

    public static ResponseResult<TValue> WithErrors<TValue>(HttpStatusCode statusCode, CustomError[] errors) => new(statusCode, errors);
}

public sealed class ResponseResult<TValue>(HttpStatusCode statusCode, CustomError[] errors)
    : Result<TValue>(false, IResponseResult.ResponseError, default), IResponseResult
{
    public HttpStatusCode StatusCode => statusCode;
    public CustomError[] Errors => errors;
}
