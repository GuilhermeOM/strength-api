using System.Net;
using Strength.Application.Muscle.Commands.UpdateMuscle;
using Strength.Domain.Repositories;
using Strength.Domain.Repositories.Base;
using Strength.Domain.Shared;

namespace Strength.Application.UnitTests.User.Commands;

public class UpdateMuscleCommandHandlerTests
{
    private readonly Mock<IMuscleRepository> _muscleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly UpdateMuscleCommandHandler _commandHandler;

    public UpdateMuscleCommandHandlerTests()
    {
        _muscleRepositoryMock = new Mock<IMuscleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _commandHandler = new UpdateMuscleCommandHandler(_muscleRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInternalServerError_WhenUpdateFails()
    {
        // Arrange
        var command = new UpdateMuscleCommand(Guid.NewGuid(), "muscleName");

        _muscleRepositoryMock
            .Setup(mock => mock.UpdateAsync(It.IsAny<Domain.Entities.Muscle>(), default))
            .ReturnsAsync(null as Domain.Entities.Muscle);

        // Act
        var result = (ResponseResult)await _commandHandler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Errors.Should().Contain(ProcessErrors.InternalError);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenUpdateSucceeds()
    {
        // Arrange
        var command = new UpdateMuscleCommand(Guid.NewGuid(), "muscleName");

        _muscleRepositoryMock
            .Setup(mock => mock.UpdateAsync(It.IsAny<Domain.Entities.Muscle>(), default))
            .ReturnsAsync(new Domain.Entities.Muscle());

        // Act
        var result = await _commandHandler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWorkMock.Verify(mock => mock.SaveChangesAsync(default), Times.Once);
    }
}
