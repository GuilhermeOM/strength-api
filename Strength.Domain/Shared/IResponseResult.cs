namespace Strength.Domain.Shared;

using System.Net;
using Constants;

public interface IResponseResult
{
    public static readonly CustomError ResponseError = new(ErrorConstants.ResponseFailure, "An error occurred");

    HttpStatusCode StatusCode { get; }
    CustomError[] Errors { get; }
}
