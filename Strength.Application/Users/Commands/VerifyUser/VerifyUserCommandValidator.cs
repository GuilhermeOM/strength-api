namespace Strength.Application.Users.Commands.VerifyUser;

using FluentValidation;

internal sealed class VerifyUserCommandValidator : AbstractValidator<VerifyUserCommand>
{
    public VerifyUserCommandValidator() =>
        this.RuleFor(x => x.VerificationToken)
            .NotNull()
            .NotEmpty();
}
