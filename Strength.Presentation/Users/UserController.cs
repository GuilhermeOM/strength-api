namespace Strength.Presentation.Users;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Users.Commands.CreateUser;
using Application.Users.Commands.LoginUser;
using Application.Users.Commands.VerifyUser;
using Microsoft.AspNetCore.Http;
using Strength.Presentation.Shared;
using Strength.Presentation.Users.Responses;

[Route("api/user")]
[ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status500InternalServerError)]
public class UserController(ISender sender) : ApiController(sender)
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await this.sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? this.Ok(new RegisterResponse("A verification link was sent to your email."))
            : this.HandleFailure(result);
    }

    [HttpGet("verify")]
    [ProducesResponseType(typeof(VerifyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Verify(
        [FromQuery] string verificationToken,
        CancellationToken cancellationToken)
    {
        var command = new VerifyUserCommand(verificationToken);
        var result = await this.sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? this.Ok(new VerifyResponse("User successfully verified. You can now log in!"))
            : this.HandleFailure(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDetailsResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await this.sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? this.Ok(new LoginResponse(result.Value.TokenType, result.Value.Token, result.Value.ExpiresAt))
            : this.HandleFailure(result);
    }
}
