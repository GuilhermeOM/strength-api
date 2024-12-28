namespace Strength.Application.Muscle.Commands.UpdateMuscle;

using System.Net;
using Abstractions.Messaging;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;

public class UpdateMuscleCommandHandler(IMuscleRepository muscleRepository) : ICommandHandler<UpdateMuscleCommand>
{
    public async Task<Result> Handle(UpdateMuscleCommand request, CancellationToken cancellationToken)
    {
        var muscle = new Muscle { Id = request.Id, Name = request.Name, };
        var updatedMuscle = await muscleRepository.UpdateAsync(muscle, cancellationToken);

        return updatedMuscle is null
            ? ResponseResult.WithErrors(HttpStatusCode.InternalServerError, [ProcessErrors.InternalError])
            : Result.Success();
    }
}
