namespace Strength.Infrastructure.Persistence.Repositories;

using Domain.Entities;
using Domain.Repositories;

internal sealed class UserRoleRepository(AppDataContext context) : IUserRoleRepository
{
    public async Task<Guid?> CreateUserRoleAsync(UserRole userRole, CancellationToken cancellationToken)
    {
        await context.UserRoles.AddAsync(userRole, cancellationToken);

        return userRole.Id;
    }
}
