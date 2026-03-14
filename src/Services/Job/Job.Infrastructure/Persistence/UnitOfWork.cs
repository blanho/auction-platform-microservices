using BuildingBlocks.Infrastructure.Repository;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;
using MediatR;

namespace Jobs.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<JobDbContext>, IUnitOfWork
{
    public UnitOfWork(JobDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}
