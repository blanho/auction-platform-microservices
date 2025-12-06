using MediatR;
using Common.Core.Helpers;

namespace Common.CQRS.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{

}

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
    
}
