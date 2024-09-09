namespace Strength.Application.UnitTests.Users.Commands;

using Application.Users.Commands.VerifyUser;
using Domain.Repositories;
using Strength.Domain.Shared;

public class VerifyUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> unitOfWorkMock = new();

    [Fact]
    public async Task ShouldReturnFailureWhenVerificationTokenIsNotAttachToUser()
    {
        // Arrange
        var command = new VerifyUserCommand("fakeVerificationToken");

        _ = this.userRepositoryMock.Setup(
            mock => mock.GetUserByVerificationTokenAsync(
                It.IsAny<string>(),
                default))
            .ReturnsAsync(null as User);

        var handler = new VerifyUserCommandHandler(
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        _ = result.IsFailure.Should().BeTrue();
        _ = result.Error.Should().Be(UserErrors.InvalidVerificationToken);
    }

    [Fact]
    public async Task UserVerifiedAtShouldNotBeNullWhenSuccess()
    {
        // Arrange
        var command = new VerifyUserCommand("fakeVerificationToken");
        var user = new User { Id = Guid.NewGuid(), Email = "email@test.com" };

        _ = this.userRepositoryMock.Setup(
            mock => mock.GetUserByVerificationTokenAsync(
                It.IsAny<string>(),
                default))
            .ReturnsAsync(user);

        _ = this.unitOfWorkMock.Setup(
            mock => mock.BeginTransactionAsync(
                It.IsAny<Func<Task<Result>>>(),
                default))
            .ReturnsAsync(Result.Success());

        var handler = new VerifyUserCommandHandler(
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        _ = result.IsSuccess.Should().BeTrue();
        _ = user.VerifiedAt.Should().NotBeNull();
    }
}
