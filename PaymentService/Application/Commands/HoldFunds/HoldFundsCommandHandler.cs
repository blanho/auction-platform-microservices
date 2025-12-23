using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Commands.HoldFunds;

public class HoldFundsCommandHandler : ICommandHandler<HoldFundsCommand, WalletTransactionDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<HoldFundsCommandHandler> _logger;

    public HoldFundsCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<HoldFundsCommandHandler> logger)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<WalletTransactionDto>> Handle(HoldFundsCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(request.Username);
        if (wallet == null)
            return Result.Failure<WalletTransactionDto>(Error.Create("Wallet.NotFound", "Wallet not found"));

        if (wallet.AvailableBalance < request.Amount)
            return Result.Failure<WalletTransactionDto>(Error.Create("Wallet.InsufficientBalance", "Insufficient available balance"));

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = wallet.UserId,
            Username = request.Username,
            Type = TransactionType.Hold,
            Amount = request.Amount,
            Balance = wallet.Balance,
            Status = TransactionStatus.Completed,
            Description = $"Funds held for {request.ReferenceType}",
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        wallet.HeldAmount += request.Amount;

        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Held {Amount} for {ReferenceType} {ReferenceId} for user: {Username}",
            request.Amount, request.ReferenceType, request.ReferenceId, request.Username);

        return Result.Success(_mapper.Map<WalletTransactionDto>(transaction));
    }
}
