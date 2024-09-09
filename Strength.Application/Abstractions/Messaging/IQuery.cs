namespace Strength.Application.Abstractions.Messaging;

using MediatR;
using Domain.Shared;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
