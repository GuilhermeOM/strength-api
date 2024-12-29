using Strength.Domain.Repositories.Base;

namespace Strength.Application.Muscle.Commands.UpdateMuscle;

using System.Net;
using Abstractions.Messaging;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;

public class UpdateMuscleCommandHandler(IMuscleRepository muscleRepository, IUnitOfWork unitOfWork) : ICommandHandler<UpdateMuscleCommand>
{
    public async Task<Result> Handle(UpdateMuscleCommand request, CancellationToken cancellationToken)
    {
        var muscle = new Muscle { Id = request.Id, Name = request.Name, };
        var updatedMuscle = await muscleRepository.UpdateAsync(muscle, cancellationToken);

        if (updatedMuscle is null)
        {
            return ResponseResult.WithErrors(HttpStatusCode.InternalServerError, [ProcessErrors.InternalError]);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
