namespace Strength.Application.Abstractions.Messaging;

using MediatR;
using Domain.Shared;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>> where TCommand : ICommand<TResponse>;
