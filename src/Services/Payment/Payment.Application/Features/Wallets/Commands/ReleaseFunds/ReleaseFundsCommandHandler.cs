using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Wallets.Commands.ReleaseFunds;

public class ReleaseFundsCommandHandler : ICommandHandler<ReleaseFundsCommand, WalletTransactionDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ReleaseFundsCommandHandler> _logger;
    private readonly IDistributedLock _distributedLock;

    private static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(10);

    public ReleaseFundsCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ReleaseFundsCommandHandler> logger,
        IDistributedLock distributedLock)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _distributedLock = distributedLock;
    }

    public async Task<Result<WalletTransactionDto>> Handle(ReleaseFundsCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"wallet:operation:{request.Username}";
        
        await using var lockHandle = await _distributedLock.TryAcquireAsync(
            lockKey,
            LockExpiry,
            cancellationToken);

        if (lockHandle == null)
        {
            _logger.LogWarning("Failed to acquire wallet lock for user {Username}", request.Username);
            return Result.Failure<WalletTransactionDto>(
                Error.Create("Wallet.Busy", "Wallet operation in progress. Please try again."));
        }

        var wallet = await _walletRepository.GetByUsernameAsync(request.Username);
        if (wallet == null)
            return Result.Failure<WalletTransactionDto>(Error.Create("Wallet.NotFound", "Wallet not found"));

        if (wallet.HeldAmount < request.Amount)
            return Result.Failure<WalletTransactionDto>(Error.Create("Wallet.InsufficientHeldAmount", "Held amount is less than release amount"));

        var transaction = WalletTransaction.Create(
            userId: wallet.UserId,
            username: request.Username,
            type: TransactionType.Hold,
            amount: -request.Amount,
            balanceAfter: wallet.Balance,
            description: "Funds released",
            referenceId: request.ReferenceId,
            referenceType: "Release");

        transaction.Complete();
        wallet.ReleaseFunds(request.Amount);

        try
        {
            await _walletRepository.UpdateAsync(wallet);
            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "Concurrency conflict releasing funds for user {Username}, reference {ReferenceId}. Lock may have been released prematurely.",
                request.Username, request.ReferenceId);
            return Result.Failure<WalletTransactionDto>(
                Error.Create("Wallet.ConcurrencyConflict", "Wallet was modified concurrently. Please retry."));
        }

        _logger.LogInformation("Released {Amount} for reference {ReferenceId} for user: {Username}",
            request.Amount, request.ReferenceId, request.Username);

        return Result.Success(transaction.ToDto(_mapper));
    }
}
