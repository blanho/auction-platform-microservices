using Auctions.Api.Grpc;
using Auctions.Application.Interfaces;
using Grpc.Core;
using MediatR;

namespace Auctions.Api.Services.Grpc;

public partial class AuctionGrpcService(
    IAuctionRepository auctionRepository,
    IMediator mediator,
    ILogger<AuctionGrpcService> logger)
    : AuctionGrpc.AuctionGrpcBase
{
    private readonly IAuctionRepository _auctionRepository = auctionRepository;
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<AuctionGrpcService> _logger = logger;
}
