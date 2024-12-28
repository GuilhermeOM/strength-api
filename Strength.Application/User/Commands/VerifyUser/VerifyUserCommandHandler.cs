namespace Strength.Application.User.Commands.VerifyUser;

using System.Net;
using Abstractions.Messaging;
using Domain.Errors;
using Domain.Repositories;
using Domain.Repositories.Base;
using Domain.Shared;

internal sealed class VerifyUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<VerifyUserCommand>
{
    public async Task<Result> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByVerificationTokenAsync(request.VerificationToken, cancellationToken);

        if (user is null)
        {
            return ResponseResult.WithErrors(HttpStatusCode.BadRequest, [UserErrors.InvalidVerificationToken]);
        }

        if (user.VerifiedAt is not null)
        {
            return ResponseResult.WithErrors(HttpStatusCode.Conflict, [UserErrors.AlreadyVerified]);
        }

        user.VerifiedAt = DateTime.UtcNow;

        return await unitOfWork.BeginTransactionAsync(async () =>
        {
            var updatedUser = await userRepository.UpdateAsync(user, cancellationToken);
            return updatedUser is null
                ? ResponseResult.WithErrors(HttpStatusCode.InternalServerError, [ProcessErrors.InternalError])
                : Result.Success();
        }, cancellationToken);
    }
}
