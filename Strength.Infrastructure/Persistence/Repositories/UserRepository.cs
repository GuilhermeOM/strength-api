namespace Strength.Infrastructure.Persistence.Repositories;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

internal sealed class UserRepository(AppDataContext context) : IUserRepository
{
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken) => await context
        .Users
        .Where(user => user.Email == email)
        .FirstOrDefaultAsync(cancellationToken);

    public async Task<User?> GetUserWithRolesByEmailAsync(string email, CancellationToken cancellationToken) => await context
        .Users
        .Where(user => user.Email == email)
        .Include(user => user.UserRoles)
        .ThenInclude(userRoles => userRoles.Role)
        .FirstOrDefaultAsync(cancellationToken);

    public async Task<User?> GetUserByVerificationTokenAsync(string verificationToken, CancellationToken cancellationToken) => await context
        .Users
        .Where(user => user.VerificationToken == verificationToken)
        .FirstOrDefaultAsync(cancellationToken);

    public async Task<Guid> CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        _ = await context.Users.AddAsync(user, cancellationToken);

        return user.Id;
    }

    public async Task<User?> UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        var userContext = await context.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync(cancellationToken);

        if (userContext is null)
        {
            return null;
        }

        userContext = user;

        return userContext;
    }
}
