namespace Strength.Application.UnitTests.Behaviors;

using Abstractions.Behaviors;
using Domain.Constants;
using Domain.Shared;
using FluentValidation;
using MediatR;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task ShouldReturnValidationResultWithErrorsWhenValidatorContainsErrors()
    {
        // Arrange
        static Task<Result> FakeHandler()
        {
            return Task.FromResult(Result.Success());
        }

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
    public async Task ShouldReturnResultWithSuccessWhenValidatorDoesContainsNoErrors()
    {
        // Arrange
        var expectedResult = Result.Success();

        Task<Result> FakeHandler()
        {
            return Task.FromResult(expectedResult);
        }

        var createUserCommand = new FakeCommand("1234", "12345678");

        var validationBehavior = new ValidationBehavior<FakeCommand, Result>(new List<FakeValidator> { new() });

        // Act
        var result = await validationBehavior.Handle(createUserCommand, FakeHandler, default);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResult);
    }

    private sealed record FakeCommand(string Prop1, string Prop2) : IRequest<Result>;

    private sealed class FakeValidator : AbstractValidator<FakeCommand>
    {
        public FakeValidator()
        {
            this.RuleFor(x => x.Prop1).MinimumLength(4);
            this.RuleFor(x => x.Prop2).MinimumLength(8);
        }
    }
}
