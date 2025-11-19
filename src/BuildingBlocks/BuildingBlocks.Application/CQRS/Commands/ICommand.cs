using MediatR;
using BuildingBlocks.Application.Abstractions;

namespace BuildingBlocks.Application.CQRS.Commands;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
    
}

public interface ICommand : IRequest<Result>
{

}

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{

}

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{

}
