namespace Strength.Application.Users.Commands.SendUserVerificationEmail;

using FluentValidation;

internal sealed class SendUserVerificationEmailCommandValidator : AbstractValidator<SendUserVerificationEmailCommand>
{
    public SendUserVerificationEmailCommandValidator() => this.RuleFor(x => x.Email).EmailAddress();
}
