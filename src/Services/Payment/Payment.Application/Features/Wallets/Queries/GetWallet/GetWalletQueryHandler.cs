using AutoMapper;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Wallets.Queries.GetWallet;

public class GetWalletQueryHandler : IQueryHandler<GetWalletQuery, WalletDto?>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;

    public GetWalletQueryHandler(
        IWalletRepository walletRepository,
        IMapper mapper)
    {
        _walletRepository = walletRepository;
        _mapper = mapper;
    }

    public async Task<Result<WalletDto?>> Handle(GetWalletQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(request.Username);
        return wallet?.ToDto(_mapper);
    }
}
