namespace Strength.Application.UnitTests.Users.Commands;

using System.Net;
using Application.Users.Commands.CreateUser;
using Domain.Entities.Enums;
using Domain.Repositories;
using Domain.Services.Email;
using Domain.Shared;
using FluentValidation.TestHelper;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> userRepositoryMock = new();
    private readonly Mock<IRoleRepository> roleRepositoryMock = new();
    private readonly Mock<IUserRoleRepository> userRoleRepositoryMock = new();
    private readonly Mock<IUnitOfWork> unitOfWorkMock = new();
    private readonly Mock<IEmailService> emailServiceMock = new();

    [Fact]
    public async Task ShouldReturnFailureWhenConfirmPasswordIsNotEqualToPassword()
    {
        // Arrange
        var command = new CreateUserCommand("email@test.com", "password123", "password321");
        var validator = new CreateUserCommandValidator(this.userRepositoryMock.Object);

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenEmailIsNotUnique()
    {
        // Arrange
        var command = new CreateUserCommand("email@test.com", "password123", "password123");
        var validator = new CreateUserCommandValidator(this.userRepositoryMock.Object);

        this.userRepositoryMock
            .Setup(mock => mock.GetUserByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new User { Email = command.Email });

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task ShouldReturnInternalFailureWhenUserNotCreated()
    {
        // Arrange
        var handler = new CreateUserCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object,
            this.userRoleRepositoryMock.Object,
            this.roleRepositoryMock.Object,
            this.unitOfWorkMock.Object);

        var command = new CreateUserCommand("test@example.com", "password123", "password123");

        this.userRepositoryMock
            .Setup(mock => mock.CreateUserAsync(It.IsAny<User>(), default))
            .ReturnsAsync(Guid.Empty);

        this.unitOfWorkMock
            .Setup(mock => mock.BeginTransactionAsync(It.IsAny<Func<Task<Result>>>(), default))
            .Returns<Func<Task<Result>>, CancellationToken>(async (func, _) => await func());

        // Act
        var result = (IResponseResult)await handler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Errors.Should().Contain(UserErrors.NotCreated);
    }

    [Fact]
    public async Task ShouldReturnInternalFailureWhenRoleUserNotFound()
    {
        // Arrange
        var handler = new CreateUserCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object,
            this.userRoleRepositoryMock.Object,
            this.roleRepositoryMock.Object,
            this.unitOfWorkMock.Object);

        var command = new CreateUserCommand("test@example.com", "password123", "password123");

        this.userRepositoryMock
            .Setup(mock => mock.CreateUserAsync(It.IsAny<User>(), default))
            .ReturnsAsync(Guid.NewGuid());

        this.roleRepositoryMock
            .Setup(mock => mock.GetRoleByNameAsync(It.IsAny<RoleName>(), default))
            .ReturnsAsync(null as Role);

        this.unitOfWorkMock
            .Setup(mock => mock.BeginTransactionAsync(It.IsAny<Func<Task<Result>>>(), default))
            .Returns<Func<Task<Result>>, CancellationToken>(async (func, _) => await func());

        // Act
        var result = (IResponseResult)await handler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Errors.Should().Contain(RoleErrors.NotFound);
    }

    [Fact]
    public async Task ShouldReturnInternalFailureWhenUserRoleNotCreated()
    {
        // Arrange
        var handler = new CreateUserCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object,
            this.userRoleRepositoryMock.Object,
            this.roleRepositoryMock.Object,
            this.unitOfWorkMock.Object);

        var command = new CreateUserCommand("test@example.com", "password123", "password123");

        this.userRepositoryMock
            .Setup(mock => mock.CreateUserAsync(It.IsAny<User>(), default))
            .ReturnsAsync(Guid.NewGuid());

        this.roleRepositoryMock
            .Setup(mock => mock.GetRoleByNameAsync(It.IsAny<RoleName>(), default))
            .ReturnsAsync(new Role());

        this.userRoleRepositoryMock
            .Setup(mock => mock.CreateUserRoleAsync(It.IsAny<UserRole>(), default))
            .ReturnsAsync(Guid.Empty);

        this.unitOfWorkMock
            .Setup(mock => mock.BeginTransactionAsync(It.IsAny<Func<Task<Result>>>(), default))
            .Returns<Func<Task<Result>>, CancellationToken>(async (func, _) => await func());

        // Act
        var result = (IResponseResult)await handler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Errors.Should().Contain(UserRoleErrors.NotCreated);
    }

    [Fact]
    public async Task ShouldReturnInternalFailureWhenVerificationEmailNotSent()
    {
        // Arrange
        var handler = new CreateUserCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object,
            this.userRoleRepositoryMock.Object,
            this.roleRepositoryMock.Object,
            this.unitOfWorkMock.Object);

        var command = new CreateUserCommand("test@example.com", "password123", "password123");

        this.userRepositoryMock
            .Setup(mock => mock.CreateUserAsync(It.IsAny<User>(), default))
            .ReturnsAsync(Guid.NewGuid());

        this.roleRepositoryMock
            .Setup(mock => mock.GetRoleByNameAsync(It.IsAny<RoleName>(), default))
            .ReturnsAsync(new Role());

        this.userRoleRepositoryMock
            .Setup(mock => mock.CreateUserRoleAsync(It.IsAny<UserRole>(), default))
            .ReturnsAsync(Guid.NewGuid());

        this.emailServiceMock
            .Setup(mock => mock.SendVerificationEmail(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(false);

        this.unitOfWorkMock
            .Setup(mock => mock.BeginTransactionAsync(It.IsAny<Func<Task<Result>>>(), default))
            .Returns<Func<Task<Result>>, CancellationToken>(async (func, _) => await func());

        // Act
        var result = (IResponseResult)await handler.Handle(command, default);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Errors.Should().Contain(UserErrors.VerificationEmailNotSent);
    }

    [Fact]
    public async Task ShouldReturnSuccessWhenVerificationEmailSent()
    {
        // Arrange
        var handler = new CreateUserCommandHandler(
            this.emailServiceMock.Object,
            this.userRepositoryMock.Object,
            this.userRoleRepositoryMock.Object,
            this.roleRepositoryMock.Object,
            this.unitOfWorkMock.Object);

        var command = new CreateUserCommand("test@example.com", "password123", "password123");

        this.userRepositoryMock
            .Setup(mock => mock.CreateUserAsync(It.IsAny<User>(), default))
            .ReturnsAsync(Guid.NewGuid());

        this.roleRepositoryMock
            .Setup(mock => mock.GetRoleByNameAsync(It.IsAny<RoleName>(), default))
            .ReturnsAsync(new Role());

        this.userRoleRepositoryMock
            .Setup(mock => mock.CreateUserRoleAsync(It.IsAny<UserRole>(), default))
            .ReturnsAsync(Guid.NewGuid());

        this.emailServiceMock
            .Setup(mock => mock.SendVerificationEmail(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(true);

        this.unitOfWorkMock
            .Setup(mock => mock.BeginTransactionAsync(It.IsAny<Func<Task<Result>>>(), default))
            .Returns<Func<Task<Result>>, CancellationToken>(async (func, _) => await func());

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
