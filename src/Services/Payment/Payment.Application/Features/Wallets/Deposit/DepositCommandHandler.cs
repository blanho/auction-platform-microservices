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

namespace Payment.Application.Features.Wallets.Deposit;

public class DepositCommandHandler : ICommandHandler<DepositCommand, WalletTransactionDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DepositCommandHandler> _logger;
    private readonly IDistributedLock _distributedLock;
    private readonly IAuditPublisher _auditPublisher;

    private static readonly TimeSpan LockExpiry = WalletDefaults.Lock.ExtendedExpiry;

    public DepositCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<DepositCommandHandler> logger,
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

    public async Task<Result<WalletTransactionDto>> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        var lockKey = $"wallet:operation:{request.Username}";

        await using var lockHandle = await _distributedLock.TryAcquireAsync(
            lockKey,
            LockExpiry,
            cancellationToken);

        if (lockHandle == null)
        {
            _logger.LogWarning("Failed to acquire wallet lock for deposit operation");
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.Busy);
        }

        var wallet = await _walletRepository.GetByUsernameAsync(request.Username);
        if (wallet == null)
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.NotFound);

        var balanceAfter = wallet.Balance + request.Amount;
        
        var transaction = WalletTransaction.Create(
            userId: wallet.UserId,
            username: request.Username,
            type: TransactionType.Deposit,
            amount: request.Amount,
            balanceAfter: balanceAfter,
            description: request.Description ?? "Deposit",
            paymentMethod: request.PaymentMethod);
        
        transaction.Complete();
        wallet.Deposit(request.Amount);

        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            transaction.Id,
            WalletTransactionAuditData.FromTransaction(transaction),
            AuditAction.Created,
            metadata: new Dictionary<string, object>
            {
                ["Action"] = "Deposit",
                ["Amount"] = request.Amount,
                ["NewBalance"] = wallet.Balance
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Deposit of {Amount} completed for user: {Username}", request.Amount, request.Username);

        return Result.Success(transaction.ToDto(_mapper));
    }
}
