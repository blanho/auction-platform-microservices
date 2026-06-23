using BuildingBlocks.Infrastructure.Repository;
using Catalog.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence;

public class CatalogUnitOfWork : BaseUnitOfWork<CatalogDbContext>
{
    public CatalogUnitOfWork(CatalogDbContext context, MediatR.IMediator mediator)
        : base(context, mediator)
    {
    }
}
