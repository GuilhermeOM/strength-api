namespace Strength.Application.Muscle.Commands.UpdateMuscle;

using FluentValidation;

internal sealed class UpdateMuscleCommandValidator : AbstractValidator<UpdateMuscleCommand>
{
    public UpdateMuscleCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();

        RuleFor(x => x.Name).NotNull().NotEmpty();
    }
}
