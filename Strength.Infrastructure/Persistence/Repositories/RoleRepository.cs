namespace Strength.Infrastructure.Persistence.Repositories;

using Base;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class RoleRepository(AppDataContext context) : BaseRepository<Role>(context), IRoleRepository
{
    private readonly AppDataContext context = context;

    public async Task<Role?> GetByNameAsync(RoleName name, CancellationToken cancellationToken = default) =>
        await this.context.Roles
            .Where(role => role.Name == name)
            .FirstOrDefaultAsync(cancellationToken);
}
