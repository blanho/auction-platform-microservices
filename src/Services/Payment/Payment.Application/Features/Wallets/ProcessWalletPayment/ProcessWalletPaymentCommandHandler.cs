using AutoMapper;
using BuildingBlocks.Application.Abstractions.Locking;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
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

    private static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(10);

    public ProcessWalletPaymentCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProcessWalletPaymentCommandHandler> logger,
        IDistributedLock distributedLock)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _distributedLock = distributedLock;
    }

    public async Task<Result<WalletTransactionDto>> Handle(ProcessWalletPaymentCommand request, CancellationToken cancellationToken)
    {

        var referenceIdStr = request.ReferenceId != Guid.Empty ? request.ReferenceId.ToString() : null;
        if (!string.IsNullOrEmpty(referenceIdStr))
        {
            var existingTransaction = await _transactionRepository.GetByReferenceIdAsync(
                referenceIdStr,
                TransactionType.Payment,
                cancellationToken);

            if (existingTransaction != null)
            {
                _logger.LogWarning(
                    "Duplicate payment attempt detected for ReferenceId {ReferenceId}, returning existing transaction {TransactionId}",
                    request.ReferenceId, existingTransaction.Id);
                return Result.Success(existingTransaction.ToDto(_mapper));
            }
        }

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

        var balanceAfter = wallet.Balance - request.Amount;
        var transaction = WalletTransaction.Create(
            userId: wallet.UserId,
            username: request.Username,
            type: TransactionType.Payment,
            amount: request.Amount,
            balanceAfter: balanceAfter,
            description: request.Description,
            referenceId: request.ReferenceId,
            referenceType: "Order");

        transaction.Complete();
        wallet.Withdraw(request.Amount);

        try
        {
            await _walletRepository.UpdateAsync(wallet);
            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex.GetType().Name.Contains("DbUpdate"))
        {
            _logger.LogWarning(ex,
                "Concurrency conflict processing payment for user {Username}, order {ReferenceId}. Lock may have been released prematurely.",
                request.Username, request.ReferenceId);
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.ConcurrencyConflict);
        }

        _logger.LogInformation("Payment of {Amount} processed for order {ReferenceId} for user: {Username}",
            request.Amount, request.ReferenceId, request.Username);

        return Result.Success(transaction.ToDto(_mapper));
    }
}
