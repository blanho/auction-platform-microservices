using MediatR;
using BuildingBlocks.Application.Abstractions;

namespace BuildingBlocks.Application.CQRS.Queries;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{

}

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{

}
