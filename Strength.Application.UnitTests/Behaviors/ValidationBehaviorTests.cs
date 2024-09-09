namespace Strength.Application.UnitTests.Behaviors;

using FluentValidation;
using MediatR;
using Strength.Application.Abstractions.Behaviors;
using Strength.Domain.Constants;
using Strength.Domain.Shared;

public class ValidationBehaviorTests
{
    private sealed record FakeCommand(string Prop1, string Prop2) : IRequest<Result>;
    private sealed class FakeValidator : AbstractValidator<FakeCommand>
    {
        public FakeValidator()
        {
            _ = this.RuleFor(x => x.Prop1).MinimumLength(4);
            _ = this.RuleFor(x => x.Prop2).MinimumLength(8);
        }
    }

    [Fact]
    public async Task ShouldReturnValidationResultWithErrorsWhenValidatorContainsErrors()
    {
        // Arrange
        static Task<Result> FakeHandler() => Task.FromResult(Result.Success());

        var createUserCommand = new FakeCommand("", "");

        var validationBehavior = new ValidationBehavior<FakeCommand, Result>(new List<FakeValidator>()
        {
            new()
        });

        // Act
        var result = (ValidationResult)await validationBehavior.Handle(createUserCommand, FakeHandler, default);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.IsFailure.Should().BeTrue();
        _ = result.Errors.Length.Should().Be(2);
        _ = result.Error.Code.Should().Be(ErrorConstants.VALIDATIONERROR);
    }

    [Fact]
    public async Task ShouldReturnResultWithSuccessWhenValidatorDoesContainsNoErrors()
    {
        // Arrange
        var expectedResult = Result.Success();

        Task<Result> FakeHandler() => Task.FromResult(expectedResult);

        var createUserCommand = new FakeCommand("1234", "12345678");

        var validationBehavior = new ValidationBehavior<FakeCommand, Result>(new List<FakeValidator>()
        {
            new()
        });

        // Act
        var result = await validationBehavior.Handle(createUserCommand, FakeHandler, default);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Should().BeEquivalentTo(expectedResult);
    }
}
