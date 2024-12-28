namespace Strength.Domain.Errors;

using Shared;

public static class MuscleExerciseErrors
{
    public static readonly CustomError AlreadyExists = new("MuscleExerciseErrors.AlreadyExists", "Muscle exercise combination already exists");
}
