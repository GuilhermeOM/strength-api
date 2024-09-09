namespace Strength.Domain.Repositories;

using Strength.Domain.Entities;
using Strength.Domain.Entities.Enums;

public interface IRoleRepository
{
    Task<Role?> GetRoleByNameAsync(RoleName name, CancellationToken cancellationToken = default);
}
