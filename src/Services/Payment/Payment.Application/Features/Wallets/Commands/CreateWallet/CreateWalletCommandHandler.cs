using AutoMapper;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Wallets.Commands.CreateWallet;

public class CreateWalletCommandHandler : ICommandHandler<CreateWalletCommand, WalletDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateWalletCommandHandler> _logger;

    public CreateWalletCommandHandler(
        IWalletRepository walletRepository,
        BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork unitOfWork,
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
            return Result.Failure<WalletDto>(Error.Create("Wallet.AlreadyExists", "Wallet already exists for this user"));

        var wallet = Wallet.Create(request.UserId, request.Username, "USD");

        var created = await _walletRepository.AddAsync(wallet);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Wallet created for user: {Username} ({UserId})", request.Username, request.UserId);

        return Result.Success(created.ToDto(_mapper));
    }
}
