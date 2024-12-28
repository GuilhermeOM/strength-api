namespace Strength.Application.UnitTests.User.Commands;

using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Application.User.Commands.SendUserVerificationEmail;
using Microsoft.Extensions.Caching.Memory;
using Strength.Domain.Constants.User;
using Domain.Repositories;
using Domain.Services.Email;
using Domain.Shared;
using User = Domain.Entities.User;

public partial class SendUserVerificationEmailCommandHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IMemoryCache> _memoryCacheMock = new();

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
            _emailServiceMock.Object,
            _userRepositoryMock.Object,
            _memoryCacheMock.Object);

        var inMemoryObject = (object)DateTime.UtcNow;

        _memoryCacheMock
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
            _emailServiceMock.Object,
            _userRepositoryMock.Object,
            _memoryCacheMock.Object);

        var anyMemoryObject = It.IsAny<object>();

        _memoryCacheMock
            .Setup(mock => mock.TryGetValue(It.IsAny<string>(), out anyMemoryObject))
            .Returns(false);

        _userRepositoryMock
            .Setup(mock => mock.GetByEmailAsync(It.IsAny<string>(), default))
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
            _emailServiceMock.Object,
            _userRepositoryMock.Object,
            _memoryCacheMock.Object);

        var anyMemoryObject = It.IsAny<object>();

        _memoryCacheMock
            .Setup(mock => mock.TryGetValue(It.IsAny<string>(), out anyMemoryObject))
            .Returns(false);

        _userRepositoryMock
            .Setup(mock => mock.GetByEmailAsync(It.IsAny<string>(), default))
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
            _emailServiceMock.Object,
            _userRepositoryMock.Object,
            _memoryCacheMock.Object);

        var anyMemoryObject = It.IsAny<object>();

        _memoryCacheMock
            .Setup(mock => mock.TryGetValue(It.IsAny<string>(), out anyMemoryObject))
            .Returns(false);

        _userRepositoryMock
            .Setup(mock => mock.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User());

        _emailServiceMock
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
            _emailServiceMock.Object,
            _userRepositoryMock.Object,
            _memoryCacheMock.Object);

        var anyMemoryObject = It.IsAny<object>();

        _memoryCacheMock
            .Setup(mock => mock.TryGetValue(It.IsAny<string>(), out anyMemoryObject))
            .Returns(false);

        _userRepositoryMock
            .Setup(mock => mock.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User());

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>);

        _emailServiceMock
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
