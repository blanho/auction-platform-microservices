using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Commands.ReleaseFunds;

public class ReleaseFundsCommandHandler : ICommandHandler<ReleaseFundsCommand, WalletTransactionDto>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ReleaseFundsCommandHandler> _logger;

    public ReleaseFundsCommandHandler(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ReleaseFundsCommandHandler> logger)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<WalletTransactionDto>> Handle(ReleaseFundsCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(request.Username);
        if (wallet == null)
            return Result.Failure<WalletTransactionDto>(Error.Create("Wallet.NotFound", "Wallet not found"));

        if (wallet.HeldAmount < request.Amount)
            return Result.Failure<WalletTransactionDto>(Error.Create("Wallet.InsufficientHeldAmount", "Held amount is less than release amount"));

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = wallet.UserId,
            Username = request.Username,
            Type = TransactionType.Hold,
            Amount = -request.Amount,
            Balance = wallet.Balance,
            Status = TransactionStatus.Completed,
            Description = "Funds released",
            ReferenceId = request.ReferenceId,
            ReferenceType = "Release",
            ProcessedAt = DateTimeOffset.UtcNow
        };

        wallet.HeldAmount -= request.Amount;

        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Released {Amount} for reference {ReferenceId} for user: {Username}",
            request.Amount, request.ReferenceId, request.Username);

        return Result.Success(_mapper.Map<WalletTransactionDto>(transaction));
    }
}
