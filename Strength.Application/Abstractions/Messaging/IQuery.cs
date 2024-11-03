namespace Strength.Application.Abstractions.Messaging;

using Domain.Shared;
using MediatR;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
