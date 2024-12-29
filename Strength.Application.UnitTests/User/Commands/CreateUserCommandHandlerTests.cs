namespace Strength.Application.UnitTests.User.Commands;

using System.Net;
using FluentValidation.TestHelper;
using Strength.Application.User.Commands.CreateUser;
using Domain.Entities.Enums;
using Domain.Repositories;
using Domain.Repositories.Base;
using Domain.Services.Email;
using Domain.Shared;
using User = Domain.Entities.User;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;

    private readonly CreateUserCommandHandler _commandHandler;

    public CreateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();

        _commandHandler = new CreateUserCommandHandler(
            _emailServiceMock.Object,
            _userRoleRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenConfirmPasswordIsNotEqualToPassword()
    {
        // Arrange
        var command = new CreateUserCommand("email@test.com", "password123", "password321");
        var validator = new CreateUserCommandValidator(_userRepositoryMock.Object);

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailIsNotUnique()
    {
        // Arrange
        var command = new CreateUserCommand("email@test.com", "password123", "password123");
        var validator = new CreateUserCommandValidator(_userRepositoryMock.Object);

        _userRepositoryMock
            .Setup(mock => mock.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User { Email = command.Email });

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Handle_ShouldReturnInternalFailure_WhenRoleNotFound()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", "password123", "password123");

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(It.IsAny<RoleName>(), default))
            .ReturnsAsync(null as Role);

        _unitOfWorkMock
            .Setup(mock => mock.BeginTransactionAsync(It.IsAny<Func<Task<Result>>>(), default))
            .Returns<Func<Task<Result>>, CancellationToken>(async (func, _) => await func());

        // Act
        var result = (IResponseResult)await _commandHandler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Errors.Should().Contain(RoleErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnInternalFailure_WhenUserRoleNotCreated()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", "password123", "password123");

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(It.IsAny<RoleName>(), default))
            .ReturnsAsync(new Role());

        _userRoleRepositoryMock
            .Setup(mock => mock.CreateAsync(It.IsAny<UserRole>(), default))
            .ReturnsAsync(Guid.Empty);

        _unitOfWorkMock
            .Setup(mock => mock.BeginTransactionAsync(It.IsAny<Func<Task<Result>>>(), default))
            .Returns<Func<Task<Result>>, CancellationToken>(async (func, _) => await func());

        // Act
        var result = (IResponseResult)await _commandHandler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Errors.Should().Contain(UserRoleErrors.NotCreated);
    }

    [Fact]
    public async Task Handle_ShouldReturnInternalFailure_WhenVerificationEmailNotSent()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", "password123", "password123");

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(It.IsAny<RoleName>(), default))
            .ReturnsAsync(new Role());

        _userRoleRepositoryMock
            .Setup(mock => mock.CreateAsync(It.IsAny<UserRole>(), default))
            .ReturnsAsync(Guid.NewGuid());

        _emailServiceMock
            .Setup(mock => mock.SendVerificationEmail(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(mock => mock.BeginTransactionAsync(It.IsAny<Func<Task<Result>>>(), default))
            .Returns<Func<Task<Result>>, CancellationToken>(async (func, _) => await func());

        // Act
        var result = (IResponseResult)await _commandHandler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Errors.Should().Contain(UserErrors.VerificationEmailNotSent);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenVerificationEmailSent()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", "password123", "password123");

        _roleRepositoryMock
            .Setup(mock => mock.GetByNameAsync(It.IsAny<RoleName>(), default))
            .ReturnsAsync(new Role());

        _userRoleRepositoryMock
            .Setup(mock => mock.CreateAsync(It.IsAny<UserRole>(), default))
            .ReturnsAsync(Guid.NewGuid());

        _emailServiceMock
            .Setup(mock => mock.SendVerificationEmail(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(mock => mock.BeginTransactionAsync(It.IsAny<Func<Task<Result>>>(), default))
            .Returns<Func<Task<Result>>, CancellationToken>(async (func, _) => await func());

        // Act
        var result = await _commandHandler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
