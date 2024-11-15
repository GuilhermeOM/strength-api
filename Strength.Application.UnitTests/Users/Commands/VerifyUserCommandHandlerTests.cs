namespace Strength.Application.UnitTests.Users.Commands;

using System.Net;
using Application.Users.Commands.VerifyUser;
using Domain.Repositories;
using Domain.Repositories.Base;
using Domain.Shared;

public class VerifyUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> unitOfWorkMock = new();
    private readonly Mock<IUserRepository> userRepositoryMock = new();

    [Fact]
    public async Task ShouldReturnFailureWhenVerificationTokenIsNotAttachToUser()
    {
        // Arrange
        var command = new VerifyUserCommand("fakeVerificationToken");

        this.userRepositoryMock.Setup(
                mock => mock.GetByVerificationTokenAsync(
                    It.IsAny<string>(),
                    default))
            .ReturnsAsync(null as User);

        var handler = new VerifyUserCommandHandler(
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object);

        // Act
        var result = (IResponseResult)await handler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain(UserErrors.InvalidVerificationToken);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenUserAlreadyVerified()
    {
        // Arrange
        var command = new VerifyUserCommand("fakeVerificationToken");

        this.userRepositoryMock
            .Setup(mock => mock.GetByVerificationTokenAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User()
            {
                VerifiedAt = DateTime.UtcNow
            });

        var handler = new VerifyUserCommandHandler(
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object);

        // Act
        var result = (IResponseResult)await handler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.Errors.Should().Contain(UserErrors.AlreadyVerified);
    }

    [Fact]
    public async Task UserVerifiedAtShouldNotBeNullWhenSuccess()
    {
        // Arrange
        var command = new VerifyUserCommand("fakeVerificationToken");
        var user = new User { Id = Guid.NewGuid(), Email = "email@test.com" };

        this.userRepositoryMock
            .Setup(mock => mock.GetByVerificationTokenAsync(It.IsAny<string>(), default))
            .ReturnsAsync(user);

        this.unitOfWorkMock
            .Setup(mock => mock.BeginTransactionAsync(It.IsAny<Func<Task<Result>>>(), default))
            .ReturnsAsync(Result.Success());

        var handler = new VerifyUserCommandHandler(
            this.userRepositoryMock.Object,
            this.unitOfWorkMock.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.VerifiedAt.Should().NotBeNull();
    }
}
