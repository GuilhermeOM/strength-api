namespace Strength.Domain.Entities;

using Base;

public class MuscleExercise : BaseEntity
{
    public required Muscle Muscle { get; init; }
    public required Exercise Exercise { get; init; }
}
