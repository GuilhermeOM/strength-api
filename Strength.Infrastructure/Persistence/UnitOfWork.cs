namespace Strength.Infrastructure.Persistence;

using Domain.Errors;
using Domain.Repositories.Base;
using Domain.Shared;

internal sealed class UnitOfWork(AppDataContext context) : IUnitOfWork
{
    public async Task<Result> BeginTransactionAsync<TResult>(
        Func<TResult> action,
        CancellationToken cancellationToken) where TResult : Task<Result>
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await action.Invoke();

            if (result.IsSuccess)
            {
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            else
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure(ProcessErrors.InternalError);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await context.SaveChangesAsync(cancellationToken);
}
