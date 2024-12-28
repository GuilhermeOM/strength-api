namespace Strength.Application.User.Commands.VerifyUser;

using FluentValidation;

internal sealed class VerifyUserCommandValidator : AbstractValidator<VerifyUserCommand>
{
    public VerifyUserCommandValidator() =>
        RuleFor(x => x.VerificationToken)
            .NotNull()
            .NotEmpty();
}
