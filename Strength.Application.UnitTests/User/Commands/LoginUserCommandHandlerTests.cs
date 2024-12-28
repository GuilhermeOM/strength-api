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
    private readonly Mock<ITokenService> _tokenProviderMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    private readonly LoginUserCommandHandler _commandHandler;

    public LoginUserCommandHandlerTests()
    {
        _tokenProviderMock = new Mock<ITokenService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _commandHandler = new LoginUserCommandHandler(
            _userRepositoryMock.Object,
            _tokenProviderMock.Object);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenEmailNotExists()
    {
        // Arrange
        var command = new LoginUserCommand("email@test.com", "password123");

        _userRepositoryMock
            .Setup(mock => mock.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(null as User);

        // Act
        var result = (IResponseResult)await _commandHandler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Errors.Should().Contain(UserErrors.NotFound);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenUserNotVerified()
    {
        // Arrange
        var command = new LoginUserCommand("email@test.com", "password123");

        HashPassword(command.Password, out var passwordHash, out var passwordSalt);

        _userRepositoryMock
            .Setup(mock => mock.GetWithRolesByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User
            {
                VerifiedAt = null,
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash
            });

        // Act
        var result = (IResponseResult)await _commandHandler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Errors.Should().Contain(UserErrors.NotVerified);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenPasswordDoesNotMatch()
    {
        // Arrange
        var command = new LoginUserCommand("email@test.com", "password123");
        const string wrongPassword = "password123";

        HashPassword(wrongPassword, out var passwordSalt, out var passwordHash);

        _userRepositoryMock
            .Setup(mock => mock.GetWithRolesByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User
            {
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash
            });

        // Act
        var result = (IResponseResult)await _commandHandler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Errors.Should().Contain(UserErrors.InvalidPassword);
    }

    [Fact]
    public async Task ShouldReturnTokenWhenPasswordMatches()
    {
        // Arrange
        var command = new LoginUserCommand("email@test.com", "password123");

        HashPassword(command.Password, out var passwordHash, out var passwordSalt);

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

        // Act
        var result = await _commandHandler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    private static void HashPassword(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
}
