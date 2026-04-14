using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Locking;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.DTOs.Audit;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Constants;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Wallets.ReleaseFunds;

public class ReleaseFundsCommandHandler : ICommandHandler<ReleaseFundsCommand, WalletTransactionDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ReleaseFundsCommandHandler> _logger;
    private readonly IDistributedLock _distributedLock;
    private readonly IAuditPublisher _auditPublisher;

    private static readonly TimeSpan LockExpiry = WalletDefaults.Lock.StandardExpiry;

    public ReleaseFundsCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ReleaseFundsCommandHandler> logger,
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

    public async Task<Result<WalletTransactionDto>> Handle(ReleaseFundsCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"wallet:operation:{request.Username}";
        
        await using var lockHandle = await _distributedLock.TryAcquireAsync(
            lockKey,
            LockExpiry,
            cancellationToken);

        if (lockHandle == null)
        {
            _logger.LogWarning("Failed to acquire wallet lock for release operation");
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.Busy);
        }

        var wallet = await _walletRepository.GetByUsernameAsync(request.Username);
        if (wallet == null)
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.NotFound);

        if (wallet.HeldAmount < request.Amount)
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.InsufficientHeldAmount);

        var transaction = WalletTransaction.Create(
            userId: wallet.UserId,
            username: request.Username,
            type: TransactionType.Release,
            amount: -request.Amount,
            balanceAfter: wallet.Balance,
            description: WalletTransactionDescriptions.FundsReleased,
            referenceId: request.ReferenceId,
            referenceType: WalletReferenceTypes.Release);

        transaction.Complete();
        wallet.ReleaseFunds(request.Amount);

        try
        {
            await _walletRepository.UpdateAsync(wallet);
            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex.GetType().Name.EndsWith("ConcurrencyException", StringComparison.Ordinal))
        {
            _logger.LogWarning(ex,
                "Concurrency conflict releasing funds for reference {ReferenceId}. Lock may have been released prematurely.",
                request.ReferenceId);
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.ConcurrencyConflict);
        }

        await _auditPublisher.PublishAsync(
            transaction.Id,
            WalletTransactionAuditData.FromTransaction(transaction),
            AuditAction.Created,
            metadata: new Dictionary<string, object>
            {
                ["Action"] = "ReleaseFunds",
                ["Amount"] = request.Amount,
                ["ReferenceId"] = request.ReferenceId,
                ["NewHeldAmount"] = wallet.HeldAmount
            },
            cancellationToken: cancellationToken);

        _logger.LogDebug("Released {Amount} for reference {ReferenceId}",
            request.Amount, request.ReferenceId);

        return Result.Success(transaction.ToDto(_mapper));
    }
}
