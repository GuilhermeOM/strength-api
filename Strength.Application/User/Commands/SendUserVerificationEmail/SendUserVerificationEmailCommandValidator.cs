namespace Strength.Application.User.Commands.SendUserVerificationEmail;

using FluentValidation;

internal sealed class SendUserVerificationEmailCommandValidator : AbstractValidator<SendUserVerificationEmailCommand>
{
    public SendUserVerificationEmailCommandValidator() => RuleFor(x => x.Email).EmailAddress();
}
