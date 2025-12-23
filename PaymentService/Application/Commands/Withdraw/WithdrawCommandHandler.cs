using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Commands.Withdraw;

public class WithdrawCommandHandler : ICommandHandler<WithdrawCommand, WalletTransactionDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<WithdrawCommandHandler> _logger;

    public WithdrawCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<WithdrawCommandHandler> logger)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<WalletTransactionDto>> Handle(WithdrawCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(request.Username);
        if (wallet == null)
            return Result.Failure<WalletTransactionDto>(Error.Create("Wallet.NotFound", "Wallet not found"));

        if (wallet.AvailableBalance < request.Amount)
            return Result.Failure<WalletTransactionDto>(Error.Create("Wallet.InsufficientBalance", "Insufficient balance"));

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = wallet.UserId,
            Username = request.Username,
            Type = TransactionType.Withdrawal,
            Amount = request.Amount,
            Balance = wallet.Balance - request.Amount,
            Status = TransactionStatus.Pending,
            Description = request.Description ?? "Withdrawal",
            PaymentMethod = request.PaymentMethod
        };

        wallet.Balance -= request.Amount;

        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Withdrawal of {Amount} initiated for user: {Username}", request.Amount, request.Username);

        return Result.Success(_mapper.Map<WalletTransactionDto>(transaction));
    }
}
