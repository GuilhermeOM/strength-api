namespace Strength.Application.UnitTests.Users.Commands;

using System.Net;
using Application.Users.Commands.SendUserVerificationEmail;
using Domain.Repositories;
using Domain.Shared;
using Domain.Services.Email;

public class SendUserVerificationEmailCommandHandlerTests
{
    private readonly Mock<IEmailService> emailServiceMock = new();
    private readonly Mock<IUserRepository> userRepositoryMock = new();

    [Fact]
    public async Task ShouldReturnFailureWhenEmailNotExists()
    {
        // Arrange
        var command = new SendUserVerificationEmailCommand("email@test.com");

        this.userRepositoryMock
            .Setup(mock => mock.GetUserByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(null as User);

        var handler = new SendUserVerificationEmailCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object);

        // Act
        var result = (IResponseResult)await handler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Errors.Should().Contain(UserErrors.NotFound);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenUserAlreadyVerified()
    {
        // Arrange
        var command = new SendUserVerificationEmailCommand("email@test.com");

        this.userRepositoryMock
            .Setup(mock => mock.GetUserByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User { VerifiedAt = DateTime.UtcNow });

        var handler = new SendUserVerificationEmailCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object);

        // Act
        var result = (IResponseResult)await handler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.Errors.Should().Contain(UserErrors.AlreadyVerified);
    }

    [Fact]
    public async Task ShouldReturnInternalFailureWhenEmailNotSent()
    {
        // Arrange
        var command = new SendUserVerificationEmailCommand("email@test.com");

        this.userRepositoryMock
            .Setup(mock => mock.GetUserByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User());

        this.emailServiceMock
            .Setup(mock => mock.SendVerificationEmail(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(false);

        var handler = new SendUserVerificationEmailCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object);

        // Act
        var result = (IResponseResult)await handler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Errors.Should().Contain(UserErrors.VerificationEmailNotSent);
    }

    [Fact]
    public async Task ShouldReturnSuccessWhenEmailSent()
    {
        // Arrange
        var command = new SendUserVerificationEmailCommand("email@test.com");

        this.userRepositoryMock
            .Setup(mock => mock.GetUserByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User());

        this.emailServiceMock
            .Setup(mock => mock.SendVerificationEmail(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(true);

        var handler = new SendUserVerificationEmailCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
