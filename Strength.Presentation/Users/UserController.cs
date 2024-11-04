namespace Strength.Presentation.Users;

using Application.Users.Commands.CreateUser;
using Application.Users.Commands.LoginUser;
using Application.Users.Commands.SendUserVerificationEmail;
using Application.Users.Commands.VerifyUser;
using Domain.Constants.User;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Responses;

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
        var result = await this.sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? this.Ok("User successfully verified. You can now log in!")
            : this.HandleFailure(result);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await this.sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? this.Ok("A verification link was sent to your email.")
            : this.HandleFailure(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await this.sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? this.Ok(new LoginResponse(result.Value.TokenType, result.Value.Token, result.Value.ExpiresAt))
            : this.HandleFailure(result);
    }

    [HttpPost("send-verification-email")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendVerificationEmailAsync(
        [FromBody] SendUserVerificationEmailCommand command,
        CancellationToken cancellationToken)
    {
        var result = await this.sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? this.Ok(result.Value)
            : this.HandleFailure(result);
    }
}
