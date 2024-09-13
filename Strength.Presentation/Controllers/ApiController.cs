namespace Strength.Presentation.Controllers;

using System.Net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Strength.Domain.Shared;

[ApiController]
public abstract class ApiController(ISender sender) : ControllerBase
{
    protected readonly ISender sender = sender;

    protected IActionResult HandleFailure(Result result)
    {
        try
        {
            var responseError = (IResponseResult)result;

            return responseError.StatusCode switch
            {
                HttpStatusCode.OK => throw new InvalidOperationException("200 is not a valid error status code"),
                HttpStatusCode.BadRequest => this.BadRequest(
                    CreateProblemDetails(nameof(StatusCodes.Status400BadRequest), StatusCodes.Status400BadRequest, responseError.Errors)),
                HttpStatusCode.NotFound => this.NotFound(
                    CreateProblemDetails(nameof(StatusCodes.Status404NotFound), StatusCodes.Status404NotFound, responseError.Errors)),
                HttpStatusCode.Conflict => this.Conflict(
                    CreateProblemDetails(nameof(StatusCodes.Status409Conflict), StatusCodes.Status409Conflict, responseError.Errors)),
                _ => this.StatusCode(StatusCodes.Status500InternalServerError,
                    CreateProblemDetails(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError, responseError.Errors)),
            };
        }
        catch (Exception)
        {
            return this.StatusCode(StatusCodes.Status500InternalServerError,
                CreateProblemDetails(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError, [result.Error]));
        }
    }

    private static ProblemDetails CreateProblemDetails(
        string title,
        int status,
        CustomError[] errors) => new()
        {
            Title = title,
            Status = status,
            Extensions = { { nameof(errors), errors } }
        };
}
