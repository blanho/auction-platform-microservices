using AutoMapper;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Wallets.CreateWallet;

public class CreateWalletCommandHandler : ICommandHandler<CreateWalletCommand, WalletDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateWalletCommandHandler> _logger;

    public CreateWalletCommandHandler(
        IWalletRepository walletRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateWalletCommandHandler> logger)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<WalletDto>> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
    {
        var exists = await _walletRepository.ExistsAsync(request.Username);
        if (exists)
            return Result.Failure<WalletDto>(PaymentErrors.Wallet.AlreadyExists);

        var wallet = Wallet.Create(request.UserId, request.Username, "USD");

        var created = await _walletRepository.AddAsync(wallet);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Wallet created for user");

        return Result.Success(created.ToDto(_mapper));
    }
}
