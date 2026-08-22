using Auctions.Application.Interfaces;
using BuildingBlocks.Application.Localization;
using Grpc.Core;

namespace Auctions.Api.Grpc;

public partial class AuctionGrpcService(
    IAuctionReadRepository readRepository,
    IAuctionWriteRepository auctionWriteRepository,
    ILogger<AuctionGrpcService> logger,
    ILocalizationService localization)
    : AuctionGrpc.AuctionGrpcBase
{
    private readonly IAuctionReadRepository _readRepository = readRepository;
    private readonly IAuctionWriteRepository _auctionWriteRepository = auctionWriteRepository;
    private readonly ILogger<AuctionGrpcService> _logger = logger;
    private readonly ILocalizationService _localization = localization;
}
