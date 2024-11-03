namespace Strength.Infrastructure.Services;

using Domain.Constants.User;
using Domain.Services.Email;
using FluentEmail.Core;
using Microsoft.AspNetCore.Http;

internal sealed class EmailService(
    IHttpContextAccessor httpContextAccessor,
    IFluentEmail fluentEmail) : IEmailService
{
    public async Task<bool> SendVerificationEmail(
        string email,
        string verificationToken,
        CancellationToken cancellationToken = default)
    {
        var verificationLink = this.CreateVerificationLink(verificationToken);
        var sendResponse = await fluentEmail
            .To(email)
            .Subject("Email verification for Strength")
            .Body($"To verify your email address <a href='{verificationLink}'>click here</a>", true)
            .SendAsync(cancellationToken);

        return sendResponse.Successful;
    }

    private string CreateVerificationLink(string verificationToken)
    {
        var httpContextRequest = httpContextAccessor.HttpContext?.Request;

        return
            $"{httpContextRequest?.Scheme}://{httpContextRequest?.Host}/api/user/{UserControllerConstants.VerifyEndpointName}?verificationToken={verificationToken}";
    }
}
