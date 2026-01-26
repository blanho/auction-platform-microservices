using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Wallets.Commands.HoldFunds;

public class HoldFundsCommandHandler : ICommandHandler<HoldFundsCommand, WalletTransactionDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<HoldFundsCommandHandler> _logger;
    private readonly IDistributedLock _distributedLock;

    private static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(30);

    public HoldFundsCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<HoldFundsCommandHandler> logger,
        IDistributedLock distributedLock)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _distributedLock = distributedLock;
    }

    public async Task<Result<WalletTransactionDto>> Handle(HoldFundsCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"wallet:operation:{request.Username}";
        
        await using var lockHandle = await _distributedLock.TryAcquireAsync(
            lockKey,
            LockExpiry,
            cancellationToken);

        if (lockHandle == null)
        {
            _logger.LogWarning("Failed to acquire wallet lock for user {Username}", request.Username);
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.Busy);
        }

        var wallet = await _walletRepository.GetByUsernameAsync(request.Username);
        if (wallet == null)
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.NotFound);

        if (wallet.AvailableBalance < request.Amount)
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.InsufficientBalance);

        var transaction = WalletTransaction.Create(
            userId: wallet.UserId,
            username: request.Username,
            type: TransactionType.Hold,
            amount: request.Amount,
            balanceAfter: wallet.Balance,
            description: $"Funds held for {request.ReferenceType}",
            referenceId: request.ReferenceId,
            referenceType: request.ReferenceType);
        
        transaction.Complete();
        wallet.HoldFunds(request.Amount);

        try
        {
            await _walletRepository.UpdateAsync(wallet);
            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "Concurrency conflict holding funds for user {Username}, reference {ReferenceId}. Lock may have been released prematurely.",
                request.Username, request.ReferenceId);
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.ConcurrencyConflict);
        }

        _logger.LogInformation("Held {Amount} for {ReferenceType} {ReferenceId} for user: {Username}",
            request.Amount, request.ReferenceType, request.ReferenceId, request.Username);

        return Result.Success(transaction.ToDto(_mapper));
    }
}
