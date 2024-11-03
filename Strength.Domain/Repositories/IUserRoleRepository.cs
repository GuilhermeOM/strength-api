namespace Strength.Domain.Repositories;

using Entities;

public interface IUserRoleRepository
{
    Task<Guid?> CreateUserRoleAsync(UserRole userRole, CancellationToken cancellationToken = default);
}
