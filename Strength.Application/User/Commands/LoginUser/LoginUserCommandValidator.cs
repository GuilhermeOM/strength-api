namespace Strength.Application.User.Commands.LoginUser;

using FluentValidation;

internal sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator() => RuleFor(x => x.Email).EmailAddress();
}
