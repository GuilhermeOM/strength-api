namespace Strength.Presentation;

using System.Net;
using Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public abstract class ApiController(ISender sender) : ControllerBase
{
    protected readonly ISender Sender = sender;

    protected IActionResult HandleFailure(Result result)
    {
        try
        {
            var responseError = (IResponseResult)result;

            return responseError.StatusCode switch
            {
                HttpStatusCode.OK => throw new InvalidOperationException("200 is not a valid error"),
                HttpStatusCode.BadRequest => BadRequest(new ErrorDetailsResponse(nameof(StatusCodes.Status400BadRequest), StatusCodes.Status400BadRequest, responseError.Errors)),
                HttpStatusCode.Unauthorized => Unauthorized(new ErrorDetailsResponse(nameof(StatusCodes.Status401Unauthorized), StatusCodes.Status401Unauthorized, responseError.Errors)),
                HttpStatusCode.NotFound => NotFound(new ErrorDetailsResponse(nameof(StatusCodes.Status404NotFound), StatusCodes.Status404NotFound, responseError.Errors)),
                HttpStatusCode.Conflict => Conflict(new ErrorDetailsResponse(nameof(StatusCodes.Status409Conflict), StatusCodes.Status409Conflict, responseError.Errors)),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new ErrorDetailsResponse(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError, responseError.Errors))
            };
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorDetailsResponse(nameof(StatusCodes.Status500InternalServerError),
                    StatusCodes.Status500InternalServerError, [result.Error]));
        }
    }
}

internal sealed record ErrorDetailsResponse(string Title, int Status, CustomError[] Errors);
