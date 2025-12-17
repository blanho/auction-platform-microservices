using AutoMapper;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<WalletService> logger)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<WalletDto?> GetWalletAsync(string username, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(username);
        return wallet != null ? _mapper.Map<WalletDto>(wallet) : null;
    }

    public async Task<WalletDto> CreateWalletAsync(Guid userId, string username, CancellationToken cancellationToken = default)
    {
        var exists = await _walletRepository.ExistsAsync(username);
        if (exists)
            throw new InvalidOperationException("Wallet already exists for this user");

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Username = username,
            Balance = 0,
            HeldAmount = 0,
            Currency = "USD",
            IsActive = true
        };

        var created = await _walletRepository.AddAsync(wallet);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Wallet created for user: {Username} ({UserId})", username, userId);

        return _mapper.Map<WalletDto>(created);
    }

    public async Task<WalletTransactionDto> DepositAsync(string username, decimal amount, string? paymentMethod, string? description, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(username);
        if (wallet == null)
            throw new KeyNotFoundException("Wallet not found");

        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = wallet.UserId,
            Username = username,
            Type = TransactionType.Deposit,
            Amount = amount,
            Balance = wallet.Balance + amount,
            Status = TransactionStatus.Completed,
            Description = description ?? "Deposit",
            PaymentMethod = paymentMethod,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        wallet.Balance += amount;
        
        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deposit of {Amount} completed for user: {Username}", amount, username);

        return _mapper.Map<WalletTransactionDto>(transaction);
    }

    public async Task<WalletTransactionDto> WithdrawAsync(string username, decimal amount, string? paymentMethod, string? description, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(username);
        if (wallet == null)
            throw new KeyNotFoundException("Wallet not found");

        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");

        if (wallet.AvailableBalance < amount)
            throw new InvalidOperationException("Insufficient balance");

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = wallet.UserId,
            Username = username,
            Type = TransactionType.Withdrawal,
            Amount = amount,
            Balance = wallet.Balance - amount,
            Status = TransactionStatus.Pending,
            Description = description ?? "Withdrawal",
            PaymentMethod = paymentMethod
        };

        wallet.Balance -= amount;
        
        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Withdrawal of {Amount} initiated for user: {Username}", amount, username);

        return _mapper.Map<WalletTransactionDto>(transaction);
    }

    public async Task<WalletTransactionDto> HoldFundsAsync(string username, decimal amount, Guid referenceId, string referenceType, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(username);
        if (wallet == null)
            throw new KeyNotFoundException("Wallet not found");

        if (wallet.AvailableBalance < amount)
            throw new InvalidOperationException("Insufficient available balance");

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = wallet.UserId,
            Username = username,
            Type = TransactionType.Hold,
            Amount = amount,
            Balance = wallet.Balance,
            Status = TransactionStatus.Completed,
            Description = $"Funds held for {referenceType}",
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        wallet.HeldAmount += amount;
        
        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Held {Amount} for {ReferenceType} {ReferenceId} for user: {Username}", 
            amount, referenceType, referenceId, username);

        return _mapper.Map<WalletTransactionDto>(transaction);
    }

    public async Task<WalletTransactionDto> ReleaseFundsAsync(string username, decimal amount, Guid referenceId, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(username);
        if (wallet == null)
            throw new KeyNotFoundException("Wallet not found");

        if (wallet.HeldAmount < amount)
            throw new InvalidOperationException("Held amount is less than release amount");

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = wallet.UserId,
            Username = username,
            Type = TransactionType.Hold,
            Amount = -amount,
            Balance = wallet.Balance,
            Status = TransactionStatus.Completed,
            Description = "Funds released",
            ReferenceId = referenceId,
            ReferenceType = "Release",
            ProcessedAt = DateTimeOffset.UtcNow
        };

        wallet.HeldAmount -= amount;
        
        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Released {Amount} for reference {ReferenceId} for user: {Username}", 
            amount, referenceId, username);

        return _mapper.Map<WalletTransactionDto>(transaction);
    }

    public async Task<WalletTransactionDto> ProcessPaymentAsync(string username, decimal amount, Guid referenceId, string description, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(username);
        if (wallet == null)
            throw new KeyNotFoundException("Wallet not found");

        if (wallet.AvailableBalance < amount)
            throw new InvalidOperationException("Insufficient balance");

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = wallet.UserId,
            Username = username,
            Type = TransactionType.Payment,
            Amount = amount,
            Balance = wallet.Balance - amount,
            Status = TransactionStatus.Completed,
            Description = description,
            ReferenceId = referenceId,
            ReferenceType = "Order",
            ProcessedAt = DateTimeOffset.UtcNow
        };

        wallet.Balance -= amount;
        
        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment of {Amount} processed for order {ReferenceId} for user: {Username}", 
            amount, referenceId, username);

        return _mapper.Map<WalletTransactionDto>(transaction);
    }

    public async Task<(IEnumerable<WalletTransactionDto> Transactions, int TotalCount)> GetTransactionsAsync(string username, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetByUsernameAsync(username, page, pageSize);
        var totalCount = await _transactionRepository.GetCountByUsernameAsync(username);
        return (_mapper.Map<IEnumerable<WalletTransactionDto>>(transactions), totalCount);
    }

    public async Task<WalletTransactionDto?> GetTransactionByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        return transaction != null ? _mapper.Map<WalletTransactionDto>(transaction) : null;
    }
}
