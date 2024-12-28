namespace Strength.Application.User.Commands.CreateUser;

using FluentValidation;
using Domain.Errors;
using Domain.Repositories;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Email)
            .MustAsync(async (email, cancellationToken) =>
                await userRepository.GetByEmailAsync(email, cancellationToken) is null)
            .WithMessage(UserErrors.AlreadyExists.Description);

        RuleFor(x => x.Password).MinimumLength(8);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage(UserErrors.InvalidConfirmPassword.Description);
    }
}
