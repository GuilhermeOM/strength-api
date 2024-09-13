namespace Strength.Domain.Shared;

using System.Net;
using Strength.Domain.Constants;

public interface IResponseResult
{
    public static readonly CustomError ResponseError = new(
        ErrorConstants.RESPONSEFAILURE,
        "An error occurred");

    HttpStatusCode StatusCode { get; }
    CustomError[] Errors { get; }
}
