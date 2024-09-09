namespace Strength.Application.UnitTests.Users.Commands;

using Application.Users.Commands.CreateUser;
using Domain.Repositories;
using FluentValidation.TestHelper;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> userRepositoryMock = new();

    [Fact]
    public async Task ShouldReturnFailureWhenConfirmPasswordIsNotEqualToPassword()
    {
        // Arrange
        var command = new CreateUserCommand("email@test.com", "password123", "password321");
        var validator = new CreateUserCommandValidator(this.userRepositoryMock.Object);

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        _ = result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public async Task ShouldReturnFailureWhenEmailIsNotUnique()
    {
        // Arrange
        var command = new CreateUserCommand("email@test.com", "password123", "password123");
        var validator = new CreateUserCommandValidator(this.userRepositoryMock.Object);

        _ = this.userRepositoryMock.Setup(
            mock => mock.GetUserByEmailAsync(
                It.IsAny<string>(),
                default))
            .ReturnsAsync(new User { Email = command.Email });

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        _ = result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
