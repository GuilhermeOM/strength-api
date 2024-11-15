namespace Strength.Infrastructure.Persistence.Repositories.Base;

using Domain.Entities.Base;

internal abstract class BaseRepository<TEntity>(AppDataContext context) where TEntity : BaseEntity
{
    public async Task<Guid> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await context.AddAsync(entity, cancellationToken);
        return entity.Id;
    }

    public async Task<TEntity?> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entityContext = await context.FindAsync<TEntity>([entity.Id], cancellationToken);

        if (entityContext is null)
        {
            return null;
        }

        entityContext = entity;
        entityContext.UpdatedAt = DateTime.UtcNow;

        return entityContext;
    }
}
