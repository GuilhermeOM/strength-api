namespace Strength.Application.Users.Commands.SendUserVerificationEmail;

using System.Net;
using Abstractions.Messaging;
using Domain.Errors;
using Domain.Repositories;
using Domain.Services.Email;
using Domain.Shared;

internal sealed class SendUserVerificationEmailCommandHandler(
    IEmailService emailService,
    IUserRepository userRepository) : ICommandHandler<SendUserVerificationEmailCommand>
{
    public async Task<Result> Handle(SendUserVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return ResponseResult.WithErrors(HttpStatusCode.NotFound, [UserErrors.NotFound]);
        }

        if (user.VerifiedAt is not null)
        {
            return ResponseResult.WithErrors(HttpStatusCode.Conflict, [UserErrors.AlreadyVerified]);
        }

        return await emailService.SendVerificationEmail(user.Email, user.VerificationToken, cancellationToken)
            ? Result.Success()
            : ResponseResult.WithErrors(HttpStatusCode.InternalServerError, [UserErrors.VerificationEmailNotSent]);
    }
}
