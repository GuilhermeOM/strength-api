namespace Strength.Infrastructure.UnitTests.Persistence;

using Domain.Shared;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

public class UnitOfWorkTests
{
    private readonly Mock<AppDataContext> appDataContextMock = new(new DbContextOptions<AppDataContext>());
    private readonly Mock<DatabaseFacade> databaseFacadeMock;
    private readonly Mock<IDbContextTransaction> dbContextTransactionMock = new();

    private readonly UnitOfWork unitOfWork;

    public UnitOfWorkTests()
    {
        this.databaseFacadeMock = new Mock<DatabaseFacade>(this.appDataContextMock.Object);

        this.appDataContextMock
            .Setup(context => context.Database)
            .Returns(this.databaseFacadeMock.Object);

        this.databaseFacadeMock
            .Setup(database => database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.dbContextTransactionMock.Object);

        this.dbContextTransactionMock
            .Setup(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.dbContextTransactionMock
            .Setup(transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this.unitOfWork = new UnitOfWork(this.appDataContextMock.Object);
    }

    [Fact]
    public async Task BeginTransactionAsyncShouldCommitWhenActionSucceeds()
    {
        // Arrange
        static Task<Result> Action()
        {
            return Task.FromResult(Result.Success());
        }

        // Act
        var actualResult = await this.unitOfWork.BeginTransactionAsync(Action, CancellationToken.None);

        // Assert
        actualResult.IsSuccess.Should().BeTrue();

        this.appDataContextMock.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        this.dbContextTransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        this.dbContextTransactionMock.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task BeginTransactionAsyncShouldRollbackWhenActionFails()
    {
        // Arrange
        static Task<Result> Action()
        {
            return Task.FromResult(Result.Failure(ProcessErrors.InternalError));
        }

        // Act
        var actualResult = await this.unitOfWork.BeginTransactionAsync(Action, CancellationToken.None);

        // Assert
        Assert.False(actualResult.IsSuccess);

        this.appDataContextMock.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        this.dbContextTransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        this.dbContextTransactionMock.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BeginTransactionAsyncShouldRollbackWhenExceptionIsThrown()
    {
        // Arrange
        static Task<Result> Action()
        {
            throw new Exception();
        }

        // Act
        var actualResult = await this.unitOfWork.BeginTransactionAsync(Action, CancellationToken.None);

        // Assert
        actualResult.IsSuccess.Should().BeFalse();

        this.appDataContextMock.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        this.dbContextTransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        this.dbContextTransactionMock.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
