namespace Strength.Domain.Repositories;

using Entities;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithRolesByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetUserByVerificationTokenAsync(string verificationToken,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
}
