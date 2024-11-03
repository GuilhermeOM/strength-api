namespace Strength.Domain.Repositories;

using Entities;
using Entities.Enums;

public interface IRoleRepository
{
    Task<Role?> GetRoleByNameAsync(RoleName name, CancellationToken cancellationToken = default);
}
