namespace Strength.Application.Abstractions.Messaging;

using MediatR;
using Domain.Shared;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
