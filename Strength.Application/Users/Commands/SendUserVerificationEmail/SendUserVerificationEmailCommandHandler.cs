namespace Strength.Application.Users.Commands.SendUserVerificationEmail;

using System.Globalization;
using System.Net;
using Abstractions.Messaging;
using Domain.Constants.User;
using Domain.Errors;
using Domain.Repositories;
using Domain.Services.Email;
using Domain.Shared;
using Microsoft.Extensions.Caching.Memory;

internal sealed class SendUserVerificationEmailCommandHandler(
    IEmailService emailService,
    IUserRepository userRepository,
    IMemoryCache memoryCache) : ICommandHandler<SendUserVerificationEmailCommand, string>
{
    public async Task<Result<string>> Handle(SendUserVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var inMemoryUser = memoryCache.TryGetValue(request.Email, out var inMemoryValue);

        if (inMemoryUser)
        {
            var memoryInsertUtcTime = DateTime.Parse(inMemoryValue!.ToString()!, CultureInfo.InvariantCulture);
            var cacheExpirationTimeInMinutes =
                SendUserVerificationEmailConstants.CacheDurationInMinutes - (DateTime.UtcNow - memoryInsertUtcTime).TotalMinutes;

            return Result.Success($"Email is already sent. Try again after {Math.Round(cacheExpirationTimeInMinutes, 2)} minutes.");
        }

        var user = await userRepository.GetUserByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return ResponseResult.WithErrors<string>(HttpStatusCode.NotFound, [UserErrors.NotFound]);
        }

        if (user.VerifiedAt is not null)
        {
            return ResponseResult.WithErrors<string>(HttpStatusCode.Conflict, [UserErrors.AlreadyVerified]);
        }

        if (!await emailService.SendVerificationEmail(user.Email, user.VerificationToken, cancellationToken))
        {
            return ResponseResult.WithErrors<string>(HttpStatusCode.InternalServerError, [UserErrors.VerificationEmailNotSent]);
        }

        _ = memoryCache.Set(request.Email, DateTime.UtcNow, TimeSpan.FromMinutes(SendUserVerificationEmailConstants.CacheDurationInMinutes));

        return Result.Success("An email with a verification link was sent!");
    }
}
