using Auctions.Api.Grpc;
using Auctions.Application.Interfaces;
using Grpc.Core;

namespace Auctions.Api.Grpc;

public partial class AuctionGrpcService(
    IAuctionReadRepository auctionReadRepository,
    IAuctionWriteRepository auctionWriteRepository,
    ILogger<AuctionGrpcService> logger)
    : AuctionGrpc.AuctionGrpcBase
{
    private readonly IAuctionReadRepository _auctionReadRepository = auctionReadRepository;
    private readonly IAuctionWriteRepository _auctionWriteRepository = auctionWriteRepository;
    private readonly ILogger<AuctionGrpcService> _logger = logger;
}
