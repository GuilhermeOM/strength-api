namespace Strength.Application.MuscleExercise.Commands.CreateManyMuscleExercise;

using Abstractions.Messaging;
using Domain.Entities;

public sealed class CreateManyMuscleExerciseCommand : ICommand
{
    public string ExerciseName { get; set; } = string.Empty;
    public string ExerciseDescription { get; set; } = string.Empty;
    public ICollection<string> MuscleNames { get; set; } = [];

    public Exercise CreateExercise() => new()
    {
        Name = ExerciseName,
        Description = ExerciseDescription,
    };

    public ICollection<Muscle> CreateMuscles() => MuscleNames.Select(ToMuscle).ToArray();

    private static Muscle ToMuscle(string muscleName) => new() { Name = muscleName };
}
