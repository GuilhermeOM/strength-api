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
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMemoryCache> _memoryCacheMock;

    private readonly SendUserVerificationEmailCommandHandler _commandHandler;

    public SendUserVerificationEmailCommandHandlerTests()
    {
        var cultureInfo = CultureInfo.CreateSpecificCulture("en-US");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        _emailServiceMock = new Mock<IEmailService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _memoryCacheMock = new Mock<IMemoryCache>();

        _commandHandler = new SendUserVerificationEmailCommandHandler(
            _emailServiceMock.Object,
            _userRepositoryMock.Object,
            _memoryCacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessAndExpirationTime_WhenEmailFoundInCache()
    {
        // Arrange
        var command = new SendUserVerificationEmailCommand("email@test.com");
        var inMemoryObject = (object)DateTime.UtcNow;

        _memoryCacheMock
            .Setup(mock => mock.TryGetValue(It.IsAny<object>(), out inMemoryObject))
            .Returns(true);

        // Act
        var result = await _commandHandler.Handle(command, default);

        // Assert
        var expiration =
            Math.Round(SendUserVerificationEmailConstants.CacheDurationInMinutes - (DateTime.UtcNow - (DateTime)inMemoryObject).TotalMinutes, 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().MatchRegex(FloatingNumberRegex());

        var cacheExpirationTime = FloatingNumberRegex().Match(result.Value).Value;
        double.Parse(cacheExpirationTime, CultureInfo.InvariantCulture).Should().BeApproximately(expiration, 0.05);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailNotExists()
    {
        // Arrange
        var command = new SendUserVerificationEmailCommand("email@test.com");
        var anyMemoryObject = It.IsAny<object>();

        _memoryCacheMock
            .Setup(mock => mock.TryGetValue(It.IsAny<string>(), out anyMemoryObject))
            .Returns(false);

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
    public async Task Handle_ShouldReturnFailure_WhenUserAlreadyVerified()
    {
        // Arrange
        var command = new SendUserVerificationEmailCommand("email@test.com");
        var anyMemoryObject = It.IsAny<object>();

        _memoryCacheMock
            .Setup(mock => mock.TryGetValue(It.IsAny<string>(), out anyMemoryObject))
            .Returns(false);

        _userRepositoryMock
            .Setup(mock => mock.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User { VerifiedAt = DateTime.UtcNow });

        // Act
        var result = (IResponseResult)await _commandHandler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.Errors.Should().Contain(UserErrors.AlreadyVerified);
    }

    [Fact]
    public async Task Handle_ShouldReturnInternalFailure_WhenEmailNotSent()
    {
        // Arrange
        var command = new SendUserVerificationEmailCommand("email@test.com");
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
        var result = (IResponseResult)await _commandHandler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Errors.Should().Contain(UserErrors.VerificationEmailNotSent);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEmailSent()
    {
        // Arrange
        var command = new SendUserVerificationEmailCommand("email@test.com");
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
        var result = await _commandHandler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [GeneratedRegex(@"\d+(\.\d+)?")]
    private static partial Regex FloatingNumberRegex();
}
