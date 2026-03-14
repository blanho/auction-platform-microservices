using Auctions.Application.Interfaces;
using Grpc.Core;

namespace Auctions.Api.Grpc;

public partial class AuctionGrpcService(
    IAuctionQueryRepository auctionQueryRepository,
    IAuctionWriteRepository auctionWriteRepository,
    IAuctionUserRepository userRepository,
    IAuctionAnalyticsRepository analyticsRepository,
    ILogger<AuctionGrpcService> logger)
    : AuctionGrpc.AuctionGrpcBase
{
    private readonly IAuctionQueryRepository _auctionQueryRepository = auctionQueryRepository;
    private readonly IAuctionWriteRepository _auctionWriteRepository = auctionWriteRepository;
    private readonly IAuctionUserRepository _userRepository = userRepository;
    private readonly IAuctionAnalyticsRepository _analyticsRepository = analyticsRepository;
    private readonly ILogger<AuctionGrpcService> _logger = logger;
}
