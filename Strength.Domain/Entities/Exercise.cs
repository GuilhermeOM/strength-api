namespace Strength.Domain.Entities;

using Base;

public class Exercise : BaseEntity
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public virtual ICollection<MuscleExercise> MuscleExercises { get; init; } = [];
}
