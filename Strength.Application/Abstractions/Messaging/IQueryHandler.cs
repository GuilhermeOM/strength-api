namespace Strength.Application.Abstractions.Messaging;

using MediatR;
using Domain.Shared;

internal interface IQueryHandler<in TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQuery<TResponse>;
