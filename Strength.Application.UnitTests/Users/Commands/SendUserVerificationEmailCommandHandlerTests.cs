namespace Strength.Application.UnitTests.Users.Commands;

using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Application.Users.Commands.SendUserVerificationEmail;
using Domain.Constants.User;
using Domain.Repositories;
using Domain.Shared;
using Domain.Services.Email;
using Microsoft.Extensions.Caching.Memory;

public partial class SendUserVerificationEmailCommandHandlerTests
{
    private readonly Mock<IEmailService> emailServiceMock = new();
    private readonly Mock<IUserRepository> userRepositoryMock = new();
    private readonly Mock<IMemoryCache> memoryCacheMock = new();

    public SendUserVerificationEmailCommandHandlerTests()
    {
        var cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }

    [Fact]
    public async Task ShouldReturnSuccessAndExpirationTimeWhenEmailFoundInCache()
    {
        // Arrange
        var command = new SendUserVerificationEmailCommand("email@test.com");
        var handler = new SendUserVerificationEmailCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object,
            this.memoryCacheMock.Object);

        var inMemoryObject = (object)DateTime.UtcNow;

        this.memoryCacheMock
            .Setup(mock => mock.TryGetValue(It.IsAny<object>(), out inMemoryObject))
            .Returns(true);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        var expiration =
            Math.Round(SendUserVerificationEmailConstants.CacheDurationInMinutes - (DateTime.UtcNow - (DateTime)inMemoryObject).TotalMinutes, 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().MatchRegex(FloatingNumberRegex());

        var cacheExpirationTime = FloatingNumberRegex().Match(result.Value).Value;
        double.Parse(cacheExpirationTime, CultureInfo.InvariantCulture).Should().BeApproximately(expiration, 0.05);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenEmailNotExists()
    {
        // Arrange
        var command = new SendUserVerificationEmailCommand("email@test.com");
        var handler = new SendUserVerificationEmailCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object,
            this.memoryCacheMock.Object);

        var anyMemoryObject = It.IsAny<object>();

        this.memoryCacheMock
            .Setup(mock => mock.TryGetValue(It.IsAny<string>(), out anyMemoryObject))
            .Returns(false);

        this.userRepositoryMock
            .Setup(mock => mock.GetUserByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(null as User);

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
        var handler = new SendUserVerificationEmailCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object,
            this.memoryCacheMock.Object);

        var anyMemoryObject = It.IsAny<object>();

        this.memoryCacheMock
            .Setup(mock => mock.TryGetValue(It.IsAny<string>(), out anyMemoryObject))
            .Returns(false);

        this.userRepositoryMock
            .Setup(mock => mock.GetUserByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User { VerifiedAt = DateTime.UtcNow });

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
        var handler = new SendUserVerificationEmailCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object,
            this.memoryCacheMock.Object);

        var anyMemoryObject = It.IsAny<object>();

        this.memoryCacheMock
            .Setup(mock => mock.TryGetValue(It.IsAny<string>(), out anyMemoryObject))
            .Returns(false);

        this.userRepositoryMock
            .Setup(mock => mock.GetUserByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User());

        this.emailServiceMock
            .Setup(mock => mock.SendVerificationEmail(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(false);

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
        var handler = new SendUserVerificationEmailCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object,
            this.memoryCacheMock.Object);

        var anyMemoryObject = It.IsAny<object>();

        this.memoryCacheMock
            .Setup(mock => mock.TryGetValue(It.IsAny<string>(), out anyMemoryObject))
            .Returns(false);

        this.userRepositoryMock
            .Setup(mock => mock.GetUserByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User());

        this.memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>);

        this.emailServiceMock
            .Setup(mock => mock.SendVerificationEmail(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [GeneratedRegex(@"\d+(\.\d+)?")]
    private static partial Regex FloatingNumberRegex();
}
