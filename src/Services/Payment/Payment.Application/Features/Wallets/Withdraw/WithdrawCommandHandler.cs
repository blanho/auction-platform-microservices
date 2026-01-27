using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Wallets.Withdraw;

public class WithdrawCommandHandler : ICommandHandler<WithdrawCommand, WalletTransactionDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<WithdrawCommandHandler> _logger;
    private readonly IDistributedLock _distributedLock;

    private static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(30);

    public WithdrawCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<WithdrawCommandHandler> logger,
        IDistributedLock distributedLock)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _distributedLock = distributedLock;
    }

    public async Task<Result<WalletTransactionDto>> Handle(WithdrawCommand request, CancellationToken cancellationToken)
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

        var balanceAfter = wallet.Balance - request.Amount;
        
        var transaction = WalletTransaction.Create(
            userId: wallet.UserId,
            username: request.Username,
            type: TransactionType.Withdrawal,
            amount: request.Amount,
            balanceAfter: balanceAfter,
            description: request.Description ?? "Withdrawal",
            paymentMethod: request.PaymentMethod);

        wallet.Withdraw(request.Amount);

        try
        {
            await _walletRepository.UpdateAsync(wallet);
            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "Concurrency conflict during withdrawal for user {Username}. Lock may have been released prematurely.",
                request.Username);
            return Result.Failure<WalletTransactionDto>(PaymentErrors.Wallet.ConcurrencyConflict);
        }

        _logger.LogInformation("Withdrawal of {Amount} initiated for user: {Username}", request.Amount, request.Username);

        return Result.Success(transaction.ToDto(_mapper));
    }
}
