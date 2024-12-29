namespace Strength.Infrastructure.UnitTests.Persistence;

using Domain.Shared;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

public class UnitOfWorkTests
{
    private readonly Mock<AppDataContext> _appDataContextMock;
    private readonly Mock<IDbContextTransaction> _dbContextTransactionMock;

    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        _appDataContextMock = new Mock<AppDataContext>(new DbContextOptions<AppDataContext>());
        _dbContextTransactionMock = new Mock<IDbContextTransaction>();

        Mock<DatabaseFacade> databaseFacadeMock = new(_appDataContextMock.Object);

        _appDataContextMock
            .Setup(context => context.Database)
            .Returns(databaseFacadeMock.Object);

        databaseFacadeMock
            .Setup(database => database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_dbContextTransactionMock.Object);

        _dbContextTransactionMock
            .Setup(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dbContextTransactionMock
            .Setup(transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWork = new UnitOfWork(_appDataContextMock.Object);
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldCommit_WhenActionSucceeds()
    {
        // Act
        var actualResult = await _unitOfWork.BeginTransactionAsync(SuccessAction, CancellationToken.None);

        // Assert
        actualResult.IsSuccess.Should().BeTrue();

        _appDataContextMock.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _dbContextTransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _dbContextTransactionMock.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldRollback_WhenActionFails()
    {
        // Act
        var actualResult = await _unitOfWork.BeginTransactionAsync(FailureAction, CancellationToken.None);

        // Assert
        actualResult.IsSuccess.Should().BeFalse();

        _appDataContextMock.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _dbContextTransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _dbContextTransactionMock.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldRollback_WhenExceptionIsThrown()
    {
        // Act
        var actualResult = await _unitOfWork.BeginTransactionAsync(ExceptionAction, CancellationToken.None);

        // Assert
        actualResult.IsSuccess.Should().BeFalse();

        _appDataContextMock.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _dbContextTransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _dbContextTransactionMock.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Task<Result> SuccessAction()
    {
        return Task.FromResult(Result.Success());
    }

    private static Task<Result> FailureAction()
    {
        return Task.FromResult(Result.Failure(ProcessErrors.InternalError));
    }

    private static Task<Result> ExceptionAction()
    {
        throw new Exception();
    }
}
