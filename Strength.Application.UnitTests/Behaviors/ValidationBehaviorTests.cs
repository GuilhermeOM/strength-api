namespace Strength.Application.UnitTests.Behaviors;

using Abstractions.Behaviors;
using Domain.Constants;
using Domain.Shared;
using FluentValidation;
using MediatR;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldReturnValidationResultWithErrors_WhenValidatorContainsErrors()
    {
        // Arrange
        var createUserCommand = new FakeCommand("", "");
        var validationBehavior = new ValidationBehavior<FakeCommand, Result>(new List<FakeValidator> { new() });

        // Act
        var result = (ResponseResult)await validationBehavior.Handle(createUserCommand, FakeHandler, default);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Length.Should().Be(2);
        result.Error.Code.Should().Be(ErrorConstants.ResponseFailure);
    }

    [Fact]
    public async Task Handle_ShouldReturnResultWithSuccess_WhenValidatorDoesContainsNoErrors()
    {
        // Arrange
        var createUserCommand = new FakeCommand("1234", "12345678");
        var validationBehavior = new ValidationBehavior<FakeCommand, Result>(new List<FakeValidator> { new() });

        // Act
        var result = await validationBehavior.Handle(createUserCommand, FakeHandler, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    private static Task<Result> FakeHandler() => Task.FromResult(Result.Success());

    private sealed record FakeCommand(string Prop1, string Prop2) : IRequest<Result>;

    private sealed class FakeValidator : AbstractValidator<FakeCommand>
    {
        public FakeValidator()
        {
            RuleFor(x => x.Prop1).MinimumLength(4);
            RuleFor(x => x.Prop2).MinimumLength(8);
        }
    }
}
