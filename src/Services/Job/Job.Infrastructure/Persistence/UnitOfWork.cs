using BuildingBlocks.Infrastructure.Repository;
using MediatR;

namespace Jobs.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<JobDbContext>
{
    public UnitOfWork(JobDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}
