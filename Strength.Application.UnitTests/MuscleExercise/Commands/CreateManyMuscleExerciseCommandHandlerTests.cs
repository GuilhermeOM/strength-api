using System.Net;
using Strength.Application.MuscleExercise.Commands.CreateManyMuscleExercise;
using Strength.Domain.Repositories;
using Strength.Domain.Repositories.Base;
using Strength.Domain.Shared;

namespace Strength.Application.UnitTests.MuscleExercise.Commands;

public class CreateManyMuscleExerciseCommandHandlerTests
{
    private readonly Mock<IMuscleExerciseRepository> _muscleExerciseRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly CreateManyMuscleExerciseCommandHandler _commandHandler;

    public CreateManyMuscleExerciseCommandHandlerTests()
    {
        _muscleExerciseRepositoryMock = new Mock<IMuscleExerciseRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _commandHandler = new CreateManyMuscleExerciseCommandHandler(
            _muscleExerciseRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenMuscleExercisesAlreadyExist()
    {
        // Arrange
        const string existingExerciseName = "existingExerciseName";
        const string existingExerciseDescription = "existingExerciseDescription";
        string[] existingRelatedMuscles = ["existingMuscle"];

        var existingMuscleExercises = new List<Domain.Entities.MuscleExercise>()
        {
            new()
            {
                Exercise = new Exercise { Name = existingExerciseName, Description = existingExerciseDescription },
                Muscle = new Domain.Entities.Muscle { Name = existingRelatedMuscles.First() }
            },
        };

        var command = new CreateManyMuscleExerciseCommand
        {
            ExerciseName = existingExerciseName,
            ExerciseDescription = existingExerciseDescription,
            MuscleNames = existingRelatedMuscles,
        };

        _muscleExerciseRepositoryMock
            .Setup(mock => mock.GetByExerciseNameAsync(It.IsAny<string>(), default))
            .ReturnsAsync(existingMuscleExercises);

        // Act
        var result = (ResponseResult)await _commandHandler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain(MuscleExerciseErrors.AlreadyExists);
    }

    [Fact]
    public async Task Handle_ShouldReturnInternalServerError_WhenNoMuscleExercisesCreated()
    {
        // Arrange
        var muscleExercises = new List<Domain.Entities.MuscleExercise>
        {
            new()
            {
                Exercise = new Exercise { Name = "exerciseName", Description = "exerciseDescription" },
                Muscle = new Domain.Entities.Muscle { Name = "muscleName" }
            }
        };

        var command = new CreateManyMuscleExerciseCommand { ExerciseName = "exerciseNameCommand", ExerciseDescription = "exerciseDescriptionCommand", MuscleNames = ["muscleNameCommand"], };

        _muscleExerciseRepositoryMock
            .Setup(mock => mock.GetByExerciseNameAsync(It.IsAny<string>(), default))
            .ReturnsAsync(muscleExercises);

        _muscleExerciseRepositoryMock
            .Setup(mock => mock.CreateManyAsync(It.IsAny<Domain.Entities.MuscleExercise[]>(), default))
            .ReturnsAsync([]);

        // Act
        var result = (ResponseResult)await _commandHandler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Errors.Should().Contain(ProcessErrors.InternalError);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenMuscleExercisesCreated()
    {
        // Arrange
        var muscleExercises = new List<Domain.Entities.MuscleExercise>()
        {
            new()
            {
                Exercise = new Exercise { Name = "exerciseName", Description = "exerciseDescription" },
                Muscle = new Domain.Entities.Muscle { Name = "muscleName" }
            }
        };

        var command = new CreateManyMuscleExerciseCommand
        {
            ExerciseName = "exerciseNameCommand",
            ExerciseDescription = "exerciseDescriptionCommand",
            MuscleNames = ["muscleNameCommand"],
        };

        _muscleExerciseRepositoryMock
            .Setup(mock => mock.GetByExerciseNameAsync(It.IsAny<string>(), default))
            .ReturnsAsync(muscleExercises);

        _muscleExerciseRepositoryMock
            .Setup(mock => mock.CreateManyAsync(It.IsAny<Domain.Entities.MuscleExercise[]>(), default))
            .ReturnsAsync([Guid.NewGuid()]);

        // Act
        var result = await _commandHandler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWorkMock.Verify(mock => mock.SaveChangesAsync(default), Times.Once);
    }
}
