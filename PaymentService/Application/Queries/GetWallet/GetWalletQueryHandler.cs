using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;

namespace PaymentService.Application.Queries.GetWallet;

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
        return wallet != null ? _mapper.Map<WalletDto>(wallet) : null;
    }
}
