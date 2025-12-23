using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Commands.ProcessWalletPayment;

public class ProcessWalletPaymentCommandHandler : ICommandHandler<ProcessWalletPaymentCommand, WalletTransactionDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcessWalletPaymentCommandHandler> _logger;

    public ProcessWalletPaymentCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProcessWalletPaymentCommandHandler> logger)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<WalletTransactionDto>> Handle(ProcessWalletPaymentCommand request, CancellationToken cancellationToken)
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
            Type = TransactionType.Payment,
            Amount = request.Amount,
            Balance = wallet.Balance - request.Amount,
            Status = TransactionStatus.Completed,
            Description = request.Description,
            ReferenceId = request.ReferenceId,
            ReferenceType = "Order",
            ProcessedAt = DateTimeOffset.UtcNow
        };

        wallet.Balance -= request.Amount;

        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment of {Amount} processed for order {ReferenceId} for user: {Username}",
            request.Amount, request.ReferenceId, request.Username);

        return Result.Success(_mapper.Map<WalletTransactionDto>(transaction));
    }
}
