namespace Strength.Domain.Entities;

using Base;

public class Muscle : BaseEntity
{
    public string Name { get; init; } = string.Empty;

    public virtual ICollection<MuscleExercise> MuscleExercises { get; init; } = [];
}
