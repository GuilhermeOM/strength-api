namespace Strength.Infrastructure.Persistence.Repositories;

using Strength.Domain.Entities;
using Strength.Domain.Repositories;

internal sealed class UserRoleRepository(AppDataContext context) : IUserRoleRepository
{
    public async Task<Guid?> CreateUserRoleAsync(UserRole userRole, CancellationToken cancellationToken)
    {
        _ = await context.UserRoles.AddAsync(userRole, cancellationToken);

        return userRole.Id;
    }
}
