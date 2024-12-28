namespace Strength.Domain.Repositories;

using Base;
using Entities;

public interface IMuscleExerciseRepository : IBaseRepository<MuscleExercise>
{
    Task<IEnumerable<MuscleExercise>> GetByExerciseNameAsync(string exerciseName, CancellationToken cancellationToken = default);
}
