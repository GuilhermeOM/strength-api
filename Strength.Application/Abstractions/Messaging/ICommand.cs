namespace Strength.Application.Abstractions.Messaging;

using Domain.Shared;
using MediatR;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
