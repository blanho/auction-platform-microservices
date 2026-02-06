using Auctions.Api.Grpc;
using Auctions.Application.Interfaces;
using Grpc.Core;
using MediatR;

namespace Auctions.Api.Grpc;

public partial class AuctionGrpcService(
    IAuctionReadRepository auctionReadRepository,
    IAuctionWriteRepository auctionWriteRepository,
    IMediator mediator,
    ILogger<AuctionGrpcService> logger)
    : AuctionGrpc.AuctionGrpcBase
{
    private readonly IAuctionReadRepository _auctionReadRepository = auctionReadRepository;
    private readonly IAuctionWriteRepository _auctionWriteRepository = auctionWriteRepository;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<AuctionGrpcService> _logger = logger;
}
