using Strength.Presentation.User.Responses;

namespace Strength.Presentation.User;

using Application.User.Commands.CreateUser;
using Application.User.Commands.LoginUser;
using Application.User.Commands.SendUserVerificationEmail;
using Application.User.Commands.VerifyUser;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Strength.Domain.Constants.User;

[Route("api/user")]
[ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status500InternalServerError)]
public class UserController(ISender sender) : ApiController(sender)
{
    [HttpGet(UserControllerConstants.VerifyEndpointName)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyAsync(
        [FromQuery] string verificationToken,
        CancellationToken cancellationToken)
    {
        var command = new VerifyUserCommand(verificationToken);
        var result = await Sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok("User successfully verified. You can now log in!")
            : HandleFailure(result);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok("A verification link was sent to your email.")
            : HandleFailure(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(new LoginResponse(result.Value.TokenType, result.Value.Token, result.Value.ExpiresAt))
            : HandleFailure(result);
    }

    [HttpPost("send-verification-email")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendVerificationEmailAsync(
        [FromBody] SendUserVerificationEmailCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : HandleFailure(result);
    }
}
