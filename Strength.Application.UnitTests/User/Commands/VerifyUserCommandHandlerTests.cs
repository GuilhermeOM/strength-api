namespace Strength.Application.UnitTests.User.Commands;

using System.Net;
using Strength.Application.User.Commands.VerifyUser;
using Domain.Repositories;
using Domain.Repositories.Base;
using Domain.Shared;
using User = Domain.Entities.User;

public class VerifyUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    private readonly VerifyUserCommandHandler _commandHandler;

    public VerifyUserCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _commandHandler = new VerifyUserCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenVerificationTokenIsNotAttachToUser()
    {
        // Arrange
        var command = new VerifyUserCommand("fakeVerificationToken");

        _userRepositoryMock
            .Setup(mock => mock.GetByVerificationTokenAsync(It.IsAny<string>(), default))
            .ReturnsAsync(null as User);

        // Act
        var result = (IResponseResult)await _commandHandler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain(UserErrors.InvalidVerificationToken);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenUserAlreadyVerified()
    {
        // Arrange
        var command = new VerifyUserCommand("fakeVerificationToken");

        _userRepositoryMock
            .Setup(mock => mock.GetByVerificationTokenAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User()
            {
                VerifiedAt = DateTime.UtcNow
            });

        // Act
        var result = (IResponseResult)await _commandHandler.Handle(command, default);

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

        _userRepositoryMock
            .Setup(mock => mock.GetByVerificationTokenAsync(It.IsAny<string>(), default))
            .ReturnsAsync(user);

        _unitOfWorkMock
            .Setup(mock => mock.BeginTransactionAsync(It.IsAny<Func<Task<Result>>>(), default))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _commandHandler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.VerifiedAt.Should().NotBeNull();
    }
}
