namespace Strength.Infrastructure.Persistence.Repositories;

using Base;using Domain.Entities;
using Domain.Repositories;

internal sealed class UserRoleRepository(AppDataContext context) : BaseRepository<UserRole>(context), IUserRoleRepository;
