namespace Strength.Application.UnitTests.User.Commands;

using System.Net;
using System.Security.Cryptography;
using System.Text;
using Application.User.Commands.LoginUser;
using Domain.Repositories;
using Domain.Services.Token;
using Domain.Shared;
using User = Domain.Entities.User;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<ITokenService> _tokenProviderMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();

    [Fact]
    public async Task ShouldReturnFailureWhenEmailNotExists()
    {
        // Arrange
        var command = new LoginUserCommand("email@test.com", "password123");

        _userRepositoryMock
            .Setup(mock => mock.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(null as User);

        var handler = new LoginUserCommandHandler(
            _userRepositoryMock.Object,
            _tokenProviderMock.Object);

        // Act
        var result = (IResponseResult)await handler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Errors.Should().Contain(UserErrors.NotFound);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenUserNotVerified()
    {
        // Arrange
        var command = new LoginUserCommand("email@test.com", "password123");

        using var hmac = new HMACSHA512();
        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(command.Password));

        _userRepositoryMock
            .Setup(mock => mock.GetWithRolesByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User
            {
                Email = command.Email,
                VerifiedAt = null,
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash
            });

        var handler = new LoginUserCommandHandler(
            _userRepositoryMock.Object,
            _tokenProviderMock.Object);

        // Act
        var result = (IResponseResult)await handler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Errors.Should().Contain(UserErrors.NotVerified);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenPasswordDoesNotMatch()
    {
        // Arrange
        var command = new LoginUserCommand("email@test.com", "password123");

        using var hmac = new HMACSHA512();
        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("password321"));

        _userRepositoryMock
            .Setup(mock => mock.GetWithRolesByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User
            {
                Email = command.Email,
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash
            });

        var handler = new LoginUserCommandHandler(
            _userRepositoryMock.Object,
            _tokenProviderMock.Object);

        // Act
        var result = (IResponseResult)await handler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Errors.Should().Contain(UserErrors.InvalidPassword);
    }

    [Fact]
    public async Task ShouldReturnTokenWhenPasswordMatches()
    {
        // Arrange
        var command = new LoginUserCommand("email@test.com", "password123");

        using var hmac = new HMACSHA512();
        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(command.Password));

        _userRepositoryMock
            .Setup(mock => mock.GetWithRolesByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User
            {
                Email = command.Email,
                VerifiedAt = DateTime.Now,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            });

        _tokenProviderMock
            .Setup(mock => mock.Create(It.IsAny<User>()))
            .Returns(new AuthToken
            {
                ExpiresAt = DateTime.Now,
                Token = string.Empty,
                TokenType = string.Empty
            });

        var handler = new LoginUserCommandHandler(
            _userRepositoryMock.Object,
            _tokenProviderMock.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
