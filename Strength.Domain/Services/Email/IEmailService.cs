namespace Strength.Domain.Services.Email;

public interface IEmailService
{
    Task<bool> SendVerificationEmail(string email, string verificationToken,
        CancellationToken cancellationToken = default);
}
