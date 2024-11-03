namespace Strength.Domain.Entities;

using System.Security.Cryptography;
using Base;

public class User : BaseEntity
{
    public string Email { get; init; } = string.Empty;
    public byte[] PasswordHash { get; init; } = new byte[32];
    public byte[] PasswordSalt { get; init; } = new byte[32];
    public string VerificationToken { get; private set; } = CreateRandomToken();
    public DateTime? VerifiedAt { get; set; }
    public string? PasswordResetToken { get; init; }

    public virtual ICollection<UserRole> UserRoles { get; init; } = [];

    private static string CreateRandomToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
}
