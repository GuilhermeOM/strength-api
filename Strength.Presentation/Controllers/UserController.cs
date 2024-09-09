namespace Strength.Presentation.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Users.Commands.CreateUser;
using Application.Users.Commands.LoginUser;
using Application.Users.Commands.VerifyUser;

[Route("api/user")]
public class UserController(ISender sender) : ApiController(sender)
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await this.sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? this.Ok("A verification link was sent to your email.")
            : this.HandleFailure(result);
    }

    [HttpGet("verify")]
    public async Task<IActionResult> Verify(
        [FromQuery] string verificationToken,
        CancellationToken cancellationToken)
    {
        var command = new VerifyUserCommand(verificationToken);

        var result = await this.sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? this.Ok("User successfully verified. You can now log in!")
            : this.HandleFailure(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await this.sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? this.Ok(result.Value)
            : this.HandleFailure(result);
    }
}
