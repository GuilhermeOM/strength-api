namespace Strength.Application.MuscleExercise.Commands.CreateManyMuscleExercise;

using System.Net;
using Abstractions.Messaging;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Repositories.Base;
using Domain.Shared;

internal sealed class CreateManyMuscleExerciseCommandHandler(
    IMuscleExerciseRepository muscleExerciseRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateManyMuscleExerciseCommand>
{
    public async Task<Result> Handle(CreateManyMuscleExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = request.CreateExercise();

        var existingMuscleExercises = await muscleExerciseRepository
            .GetByExerciseNameAsync(exercise.Name, cancellationToken);

        var muscleExercises = request
            .CreateMuscles()
            .Select(muscle => new MuscleExercise { Exercise = exercise, Muscle = muscle })
            .Select(muscleExercise => MuscleExerciseExists(existingMuscleExercises, muscleExercise) ? null : muscleExercise)
            .Where(muscleExercise => muscleExercise is not null)
            .ToArray();

        var muscleExercisesExists = muscleExercises.Length == 0;
        if (muscleExercisesExists)
        {
            return ResponseResult.WithErrors(HttpStatusCode.BadRequest, [MuscleExerciseErrors.AlreadyExists]);
        }

        var newMuscleExercises = await muscleExerciseRepository.CreateManyAsync(muscleExercises!, cancellationToken);
        if (newMuscleExercises.ToArray().Length == 0)
        {
            return ResponseResult.WithErrors(HttpStatusCode.InternalServerError, [ProcessErrors.InternalError]);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static bool MuscleExerciseExists(
        IEnumerable<MuscleExercise> existingMuscleExercises,
        MuscleExercise muscleExercise)
    {
        var exists = existingMuscleExercises.Any(existingMuscleExercise =>
            existingMuscleExercise.Exercise.Name.Equals(muscleExercise.Exercise.Name, StringComparison.OrdinalIgnoreCase)
            && existingMuscleExercise.Muscle.Name.Equals(muscleExercise.Muscle.Name, StringComparison.OrdinalIgnoreCase));

        return exists;
    }
}
