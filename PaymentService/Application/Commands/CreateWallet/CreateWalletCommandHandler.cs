using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Commands.CreateWallet;

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
            return Result.Failure<WalletDto>(Error.Create("Wallet.AlreadyExists", "Wallet already exists for this user"));

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Username = request.Username,
            Balance = 0,
            HeldAmount = 0,
            Currency = "USD",
            IsActive = true
        };

        var created = await _walletRepository.AddAsync(wallet);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Wallet created for user: {Username} ({UserId})", request.Username, request.UserId);

        return Result.Success(_mapper.Map<WalletDto>(created));
    }
}
