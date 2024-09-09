namespace Strength.Domain.Entities;

public class User
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public byte[] PasswordHash { get; init; } = new byte[32];
    public byte[] PasswordSalt { get; init; } = new byte[32];
    public string? VerificationToken { get; init; }
    public DateTime? VerifiedAt { get; set; }
    public string? PasswordResetToken { get; init; }

    public virtual ICollection<UserRole> UserRoles { get; init; } = [];
}
