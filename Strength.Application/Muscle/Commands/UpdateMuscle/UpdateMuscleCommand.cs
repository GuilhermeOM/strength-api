namespace Strength.Application.Muscle.Commands.UpdateMuscle;

using Abstractions.Messaging;

public sealed record UpdateMuscleCommand(Guid Id, string Name) : ICommand;
