namespace Strength.Domain.Repositories;

using Strength.Domain.Entities;

public interface IUserRoleRepository
{
    Task<Guid?> CreateUserRoleAsync(UserRole userRole, CancellationToken cancellationToken = default);
}
