namespace Strength.Application.Users.Commands.CreateUser;

using Domain.Errors;
using Domain.Repositories;
using FluentValidation;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserRepository userRepository)
    {
        this.RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        this.RuleFor(x => x.Email)
            .MustAsync(async (email, cancellationToken) =>
                await userRepository.GetUserByEmailAsync(email, cancellationToken) is null)
            .WithMessage(UserErrors.AlreadyExists.Description);

        this.RuleFor(x => x.Password).MinimumLength(8);

        this.RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage(UserErrors.InvalidConfirmPassword.Description);
    }
}
