namespace Strength.Infrastructure.Persistence.Repositories;

using Base;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class UserRepository(AppDataContext context) : BaseRepository<User>(context), IUserRepository
{
    private readonly AppDataContext context = context;

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        await this.context
            .Users
            .Where(user => user.Email == email)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<User?> GetByVerificationTokenAsync(string verificationToken, CancellationToken cancellationToken) =>
        await this.context
            .Users
            .Where(user => user.VerificationToken == verificationToken)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<User?> GetWithRolesByEmailAsync(string email, CancellationToken cancellationToken) =>
        await this.context
            .Users
            .Where(user => user.Email == email)
            .Include(user => user.UserRoles)
            .ThenInclude(userRoles => userRoles.Role)
            .FirstOrDefaultAsync(cancellationToken);
}
