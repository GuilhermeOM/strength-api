namespace Strength.Application.Abstractions.Messaging;

using Domain.Shared;
using MediatR;

internal interface IQueryHandler<in TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQuery<TResponse>;
