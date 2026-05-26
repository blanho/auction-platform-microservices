using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Application.Constants;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.DTOs.Audit;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Constants;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Wallets.HoldFunds;

public class HoldFundsCommandHandler : ICommandHandler<HoldFundsCommand, WalletTransactionDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<HoldFundsCommandHandler> _logger;
    private readonly IDistributedLock _distributedLock;
    private readonly IAuditPublisher _auditPublisher;

    private static readonly TimeSpan LockExpiry = WalletDefaults.Lock.ExtendedExpiry;

    public HoldFundsCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<HoldFundsCommandHandler> logger,
        IDistributedLock distributedLock,
        IAuditPublisher auditPublisher)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _distributedLock = distributedLock;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<WalletTransactionDto>> Handle(HoldFundsCommand request, CancellationToken cancellationToken)
    {
        var lockKey = WalletDefaults.Lock.GetWalletOperationKey(request.Username);
        
        await using var lockHandle = await _distributedLock.TryAcquireAsync(
            lockKey,
            LockExpiry,
            cancellationToken);

        if (lockHandle == null)
        {
            _logger.LogWarning("Failed to acquire wallet lock for operation");
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
            description: string.Format(WalletTransactionDescriptions.FundsHeldForReferenceFormat, request.ReferenceType),
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
        catch (BuildingBlocks.Domain.Exceptions.ConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "Concurrency conflict holding funds for reference {ReferenceId}. Lock may have been released prematurely.",
                request.ReferenceId);
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.ConcurrencyConflict);
        }

        await _auditPublisher.PublishAsync(
            transaction.Id,
            WalletTransactionAuditData.FromTransaction(transaction),
            AuditAction.Created,
            metadata: new Dictionary<string, object>
            {
                [AuditMetadataKeys.Action] = WalletDefaults.Audit.HoldFunds,
                [AuditMetadataKeys.Amount] = request.Amount,
                [AuditMetadataKeys.ReferenceId] = request.ReferenceId,
                [AuditMetadataKeys.ReferenceType] = request.ReferenceType,
                [AuditMetadataKeys.NewHeldAmount] = wallet.HeldAmount
            },
            cancellationToken: cancellationToken);

        _logger.LogDebug("Held {Amount} for {ReferenceType} {ReferenceId}",
            request.Amount, request.ReferenceType, request.ReferenceId);

        return Result.Success(transaction.ToDto(_mapper));
    }
}
