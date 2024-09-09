namespace Strength.Application.Users.Commands.LoginUser;

using FluentValidation;

internal sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator() => this.RuleFor(x => x.Email).EmailAddress();
}
