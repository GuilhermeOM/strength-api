namespace Strength.Infrastructure.Persistence.Repositories;

using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class RoleRepository(AppDataContext context) : IRoleRepository
{
    public async Task<Role?> GetRoleByNameAsync(RoleName name, CancellationToken cancellationToken = default) =>
        await context.Roles
            .Where(role => role.Name == name)
            .FirstOrDefaultAsync(cancellationToken);
}
