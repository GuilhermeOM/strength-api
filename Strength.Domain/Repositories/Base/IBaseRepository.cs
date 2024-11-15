namespace Strength.Domain.Repositories.Base;

using Entities.Base;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    Task<Guid> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity?> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
}
