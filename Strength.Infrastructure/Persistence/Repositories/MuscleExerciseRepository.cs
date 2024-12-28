namespace Strength.Infrastructure.Persistence.Repositories;

using Base;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

internal class MuscleExerciseRepository(AppDataContext context)
    : BaseRepository<MuscleExercise>(context), IMuscleExerciseRepository
{
    private readonly AppDataContext _context = context;

    public async Task<IEnumerable<MuscleExercise>> GetByExerciseNameAsync(string exerciseName, CancellationToken cancellationToken = default) =>
        await this._context.MuscleExercises
            .Where(muscleExercise => muscleExercise.Exercise.Name == exerciseName)
            .Include(muscleExercise => muscleExercise.Muscle)
            .Include(muscleExercise => muscleExercise.Exercise)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public override async Task<IEnumerable<Guid>> CreateManyAsync(MuscleExercise[] entities, CancellationToken cancellationToken = default)
    {
        var exerciseNames = entities
            .Select(muscleExercise => muscleExercise.Exercise.Name).Distinct();

        var muscleNames = entities
            .Select(muscleExercise => muscleExercise.Muscle.Name).Distinct();

        var existingExercises = await this._context.Exercises
            .Where(exercise => exerciseNames.Contains(exercise.Name))
            .ToDictionaryAsync(exercise => exercise.Name.ToLowerInvariant(), cancellationToken);

        var existingMuscles = await this._context.Muscles
            .Where(muscle => muscleNames.Contains(muscle.Name))
            .ToDictionaryAsync(muscle => muscle.Name.ToLowerInvariant(), cancellationToken);

        var filteredEntities = entities.Select(entity =>
        {
            var exercise = existingExercises.TryGetValue(entity.Exercise.Name.ToLowerInvariant(), out var existingExercise)
                ? existingExercise
                : entity.Exercise;

            var muscle = existingMuscles.TryGetValue(entity.Muscle.Name.ToLowerInvariant(), out var existingMuscle)
                ? existingMuscle
                : entity.Muscle;

            return new MuscleExercise { Exercise = exercise, Muscle = muscle };
        }).ToArray();

        return await base.CreateManyAsync(filteredEntities, cancellationToken);
    }
}
