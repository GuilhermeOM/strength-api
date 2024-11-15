namespace Strength.Domain.Repositories;

using Base;
using Entities;
using Entities.Enums;

public interface IRoleRepository : IBaseRepository<Role>
{
    Task<Role?> GetByNameAsync(RoleName name, CancellationToken cancellationToken = default);
}
