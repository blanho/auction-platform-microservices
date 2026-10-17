using DomainConcurrencyException = BuildingBlocks.Domain.Exceptions.ConcurrencyException;
using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Application.Constants;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Constants;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Wallets.ProcessWalletPayment;

public class ProcessWalletPaymentCommandHandler : ICommandHandler<ProcessWalletPaymentCommand, WalletTransactionDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcessWalletPaymentCommandHandler> _logger;
    private readonly IDistributedLock _distributedLock;
    private readonly IAuditPublisher _auditPublisher;
    private readonly IWalletPaymentExecutor _paymentExecutor;

    private static readonly TimeSpan LockExpiry = WalletDefaults.Lock.StandardExpiry;

    public ProcessWalletPaymentCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProcessWalletPaymentCommandHandler> logger,
        IDistributedLock distributedLock,
        IAuditPublisher auditPublisher,
        IWalletPaymentExecutor paymentExecutor)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _distributedLock = distributedLock;
        _auditPublisher = auditPublisher;
        _paymentExecutor = paymentExecutor;
    }

    public Task<Result<WalletTransactionDto>> Handle(ProcessWalletPaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.ReferenceId == Guid.Empty)
            return Task.FromResult(Result.Failure<WalletTransactionDto>(PaymentErrors.Payment.InvalidReference));
        if (request.Amount <= 0 || decimal.Round(request.Amount, WalletDefaults.Persistence.MoneyScale) != request.Amount)
            return Task.FromResult(Result.Failure<WalletTransactionDto>(PaymentErrors.Payment.InvalidAmount));

        return _paymentExecutor.ExecuteAsync(request.ReferenceId, ct => ProcessAsync(request, ct), cancellationToken);
    }

    private async Task<Result<WalletTransactionDto>> ProcessAsync(ProcessWalletPaymentCommand request, CancellationToken cancellationToken)
    {
        var existingTransaction = await _transactionRepository.GetByReferenceIdAsync(
            request.ReferenceId.ToString(),
            TransactionType.Payment,
            cancellationToken);

        if (existingTransaction != null)
            return ResolveExistingPayment(existingTransaction, request);

        var lockKey = WalletDefaults.Lock.GetWalletOperationKey(request.Username);

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
            type: TransactionType.Payment,
            amount: request.Amount,
            balanceAfter: wallet.Balance - request.Amount,
            description: request.Description,
            referenceId: request.ReferenceId,
            referenceType: WalletReferenceTypes.Order);

        transaction.Complete();
        wallet.Withdraw(request.Amount);

        try
        {
            await _walletRepository.UpdateAsync(wallet);
            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DomainConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "Concurrency conflict processing payment for user {Username}, order {ReferenceId}. Lock may have been released prematurely.",
                request.Username, request.ReferenceId);
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.ConcurrencyConflict);
        }

        await PublishPaymentAuditAsync(transaction, wallet.Balance, request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment of {Amount} processed for order {ReferenceId} for user: {Username}",
            request.Amount, request.ReferenceId, request.Username);

        return Result.Success(transaction.ToDto(_mapper));
    }

    private Result<WalletTransactionDto> ResolveExistingPayment(
        WalletTransaction transaction,
        ProcessWalletPaymentCommand request)
    {
        if (!MatchesPaymentRequest(transaction, request))
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Payment.ReferenceConflict);

        _logger.LogWarning(
            "Duplicate payment attempt detected for ReferenceId {ReferenceId}, returning existing transaction {TransactionId}",
            request.ReferenceId, transaction.Id);

        return Result.Success(transaction.ToDto(_mapper));
    }

    private static bool MatchesPaymentRequest(WalletTransaction transaction, ProcessWalletPaymentCommand request) =>
        transaction.Username == request.Username &&
        transaction.Amount == request.Amount &&
        transaction.Description == request.Description &&
        transaction.ReferenceType == WalletReferenceTypes.Order &&
        transaction.Status == TransactionStatus.Completed;

    private Task PublishPaymentAuditAsync(
        WalletTransaction transaction,
        decimal newBalance,
        ProcessWalletPaymentCommand request,
        CancellationToken cancellationToken) =>
        _auditPublisher.PublishAsync(
            transaction.Id,
            WalletTransactionAuditData.FromTransaction(transaction),
            AuditAction.Created,
            metadata: new Dictionary<string, object>
            {
                [AuditMetadataKeys.Action] = WalletDefaults.Audit.ProcessWalletPayment,
                [AuditMetadataKeys.Amount] = request.Amount,
                [AuditMetadataKeys.ReferenceId] = request.ReferenceId,
                [AuditMetadataKeys.NewBalance] = newBalance
            },
            cancellationToken: cancellationToken);
}
