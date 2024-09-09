namespace Strength.Presentation.Controllers;

using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Strength.Domain.Constants;
using Strength.Domain.Shared;

[ApiController]
public abstract class ApiController(ISender sender) : ControllerBase
{
    protected readonly ISender sender = sender;

    protected IActionResult HandleFailure(Result result) =>
        result switch
        {
            { IsSuccess: true } => throw new InvalidOperationException(),
            IValidationResult validationResult =>
                this.BadRequest(
                    CreateProblemDetails(
                        ErrorConstants.VALIDATIONERROR,
                        StatusCodes.Status400BadRequest,
                        result.Error,
                        validationResult.Errors)),
            _ =>
                this.BadRequest(
                    CreateProblemDetails(
                        ErrorConstants.VALIDATIONERROR,
                        StatusCodes.Status400BadRequest,
                        result.Error))
        };

    private static ProblemDetails CreateProblemDetails(
        string title,
        int status,
        CustomError error,
        CustomError[]? errors = null) => new()
        {
            Title = title,
            Type = error.Code,
            Detail = error.Description,
            Status = status,
            Extensions = { { nameof(errors), errors } }
        };
}
