namespace Strength.Domain.Repositories;

using Base;
using Entities;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetWithRolesByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByVerificationTokenAsync(string verificationToken, CancellationToken cancellationToken = default);
}
