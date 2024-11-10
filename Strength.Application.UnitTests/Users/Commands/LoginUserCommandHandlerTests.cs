namespace Strength.Application.UnitTests.Users.Commands;

using System.Net;
using System.Security.Cryptography;
using System.Text;
using Application.Users.Commands.LoginUser;
using Domain.Repositories;
using Domain.Services.Token;
using Domain.Shared;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<ITokenService> tokenProviderMock = new();
    private readonly Mock<IUserRepository> userRepositoryMock = new();

    [Fact]
    public async Task ShouldReturnFailureWhenEmailNotExists()
    {
        // Arrange
        var command = new LoginUserCommand("email@test.com", "password123");

        this.userRepositoryMock
            .Setup(mock => mock.GetUserByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(null as User);

        var handler = new LoginUserCommandHandler(
            this.userRepositoryMock.Object,
            this.tokenProviderMock.Object);

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

        this.userRepositoryMock
            .Setup(mock => mock.GetUserWithRolesByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User
            {
                Email = command.Email,
                VerifiedAt = null,
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash
            });

        var handler = new LoginUserCommandHandler(
            this.userRepositoryMock.Object,
            this.tokenProviderMock.Object);

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

        this.userRepositoryMock
            .Setup(mock => mock.GetUserWithRolesByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User
            {
                Email = command.Email,
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash
            });

        var handler = new LoginUserCommandHandler(
            this.userRepositoryMock.Object,
            this.tokenProviderMock.Object);

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

        this.userRepositoryMock
            .Setup(mock => mock.GetUserWithRolesByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User
            {
                Email = command.Email,
                VerifiedAt = DateTime.Now,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            });

        this.tokenProviderMock
            .Setup(mock => mock.Create(It.IsAny<User>()))
            .Returns(new AuthToken
            {
                ExpiresAt = DateTime.Now,
                Token = string.Empty,
                TokenType = string.Empty
            });

        var handler = new LoginUserCommandHandler(
            this.userRepositoryMock.Object,
            this.tokenProviderMock.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
