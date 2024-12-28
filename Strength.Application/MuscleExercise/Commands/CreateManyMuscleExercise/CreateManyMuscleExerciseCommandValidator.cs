namespace Strength.Application.MuscleExercise.Commands.CreateManyMuscleExercise;

using FluentValidation;

internal sealed class CreateManyMuscleExerciseCommandValidator : AbstractValidator<CreateManyMuscleExerciseCommand>
{
    public CreateManyMuscleExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseName)
            .NotEmpty()
            .NotNull()
            .MaximumLength(50);

        RuleFor(x => x.MuscleNames).NotNull();
        RuleFor(x => x.MuscleNames.Count).GreaterThan(0);
    }
}
