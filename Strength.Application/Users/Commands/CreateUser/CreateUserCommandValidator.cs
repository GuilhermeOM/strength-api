namespace Strength.Application.Users.Commands.CreateUser;

using FluentValidation;
using Strength.Domain.Repositories;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserRepository userRepository)
    {
        _ = this.RuleFor(x => x.Email)
            .EmailAddress();

        _ = this.RuleFor(x => x.Email)
            .MustAsync(async (email, cancellationToken) => (
                await userRepository.GetUserByEmailAsync(email, cancellationToken)) is null)
            .WithMessage("Email already exists.");

        _ = this.RuleFor(x => x.Password).MinimumLength(8);

        _ = this.RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("'Confirm Password' must be equal to 'Password'");
    }
}
