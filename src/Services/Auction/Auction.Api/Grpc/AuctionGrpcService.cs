using Auctions.Application.Interfaces;
using Grpc.Core;

namespace Auctions.Api.Grpc;

public partial class AuctionGrpcService(
    IAuctionQueryRepository auctionQueryRepository,
    IAuctionWriteRepository auctionWriteRepository,
    ILogger<AuctionGrpcService> logger)
    : AuctionGrpc.AuctionGrpcBase
{
    private readonly IAuctionQueryRepository _auctionQueryRepository = auctionQueryRepository;
    private readonly IAuctionWriteRepository _auctionWriteRepository = auctionWriteRepository;
    private readonly ILogger<AuctionGrpcService> _logger = logger;
}
