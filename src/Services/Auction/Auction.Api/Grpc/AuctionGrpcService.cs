using Auctions.Application.Interfaces;
using Grpc.Core;

namespace Auctions.Api.Grpc;

public partial class AuctionGrpcService(
    IAuctionReadRepository readRepository,
    IAuctionWriteRepository auctionWriteRepository,
    ILogger<AuctionGrpcService> logger)
    : AuctionGrpc.AuctionGrpcBase
{
    private readonly IAuctionReadRepository _readRepository = readRepository;
    private readonly IAuctionWriteRepository _auctionWriteRepository = auctionWriteRepository;
    private readonly ILogger<AuctionGrpcService> _logger = logger;
}
