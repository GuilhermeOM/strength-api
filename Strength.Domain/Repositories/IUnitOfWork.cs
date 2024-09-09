namespace Strength.Domain.Repositories;

using Shared;

public interface IUnitOfWork
{
    Task<Result> BeginTransactionAsync<TResult>(
        Func<TResult> action,
        CancellationToken cancellationToken = default) where TResult : Task<Result>;
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
