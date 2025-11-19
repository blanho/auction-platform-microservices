using AutoMapper;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Wallets.Commands.Deposit;

public class DepositCommandHandler : ICommandHandler<DepositCommand, WalletTransactionDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DepositCommandHandler> _logger;

    public DepositCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<DepositCommandHandler> logger)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<WalletTransactionDto>> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(request.Username);
        if (wallet == null)
            return Result.Failure<WalletTransactionDto>(Error.Create("Wallet.NotFound", "Wallet not found"));

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

        _logger.LogInformation("Deposit of {Amount} completed for user: {Username}", request.Amount, request.Username);

        return Result.Success(_mapper.Map<WalletTransactionDto>(transaction));
    }
}
