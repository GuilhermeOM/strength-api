namespace Strength.Infrastructure.Persistence.Repositories;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Strength.Domain.Entities;
using Strength.Domain.Entities.Enums;
using Strength.Domain.Repositories;

internal sealed class RoleRepository(AppDataContext context) : IRoleRepository
{
    public async Task<Role?> GetRoleByNameAsync(RoleName name, CancellationToken cancellationToken = default) => await context.Roles
        .Where(role => role.Name == name)
        .FirstOrDefaultAsync(cancellationToken);
}
