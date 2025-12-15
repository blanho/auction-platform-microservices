using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletsController : ControllerBase
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<WalletsController> _logger;

    public WalletsController(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IMapper mapper,
        ILogger<WalletsController> logger)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<WalletDto>> GetWallet(string username)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(username);
        if (wallet == null)
            return NotFound();

        return Ok(_mapper.Map<WalletDto>(wallet));
    }

    [HttpPost("{username}/create")]
    public async Task<ActionResult<WalletDto>> CreateWallet(string username)
    {
        var exists = await _walletRepository.ExistsAsync(username);
        if (exists)
            return Conflict("Wallet already exists for this user");

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            Username = username,
            Balance = 0,
            HeldAmount = 0,
            Currency = "USD",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var created = await _walletRepository.AddAsync(wallet);

        _logger.LogInformation("Wallet created for user: {Username}", username);

        return CreatedAtAction(nameof(GetWallet), new { username }, _mapper.Map<WalletDto>(created));
    }

    [HttpPost("{username}/deposit")]
    public async Task<ActionResult<WalletTransactionDto>> Deposit(string username, [FromBody] DepositDto dto)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(username);
        if (wallet == null)
            return NotFound("Wallet not found");

        if (dto.Amount <= 0)
            return BadRequest("Amount must be positive");

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            Username = username,
            Type = TransactionType.Deposit,
            Amount = dto.Amount,
            Balance = wallet.Balance + dto.Amount,
            Status = TransactionStatus.Completed,
            Description = dto.Description ?? "Deposit",
            PaymentMethod = dto.PaymentMethod,
            CreatedAt = DateTimeOffset.UtcNow,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        wallet.Balance += dto.Amount;
        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);

        _logger.LogInformation("Deposit of {Amount} completed for user: {Username}", dto.Amount, username);

        return Ok(_mapper.Map<WalletTransactionDto>(transaction));
    }

    [HttpPost("{username}/withdraw")]
    public async Task<ActionResult<WalletTransactionDto>> Withdraw(string username, [FromBody] WithdrawDto dto)
    {
        var wallet = await _walletRepository.GetByUsernameAsync(username);
        if (wallet == null)
            return NotFound("Wallet not found");

        if (dto.Amount <= 0)
            return BadRequest("Amount must be positive");

        if (wallet.AvailableBalance < dto.Amount)
            return BadRequest("Insufficient balance");

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            Username = username,
            Type = TransactionType.Withdrawal,
            Amount = dto.Amount,
            Balance = wallet.Balance - dto.Amount,
            Status = TransactionStatus.Pending,
            Description = dto.Description ?? "Withdrawal",
            PaymentMethod = dto.PaymentMethod,
            CreatedAt = DateTimeOffset.UtcNow
        };

        wallet.Balance -= dto.Amount;
        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);

        _logger.LogInformation("Withdrawal of {Amount} initiated for user: {Username}", dto.Amount, username);

        return Ok(_mapper.Map<WalletTransactionDto>(transaction));
    }

    [HttpGet("{username}/transactions")]
    public async Task<ActionResult<IEnumerable<WalletTransactionDto>>> GetTransactions(
        string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var transactions = await _transactionRepository.GetByUsernameAsync(username, page, pageSize);
        var totalCount = await _transactionRepository.GetCountByUsernameAsync(username);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        Response.Headers.Append("X-Page", page.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        return Ok(_mapper.Map<IEnumerable<WalletTransactionDto>>(transactions));
    }

    [HttpGet("transactions/{id:guid}")]
    public async Task<ActionResult<WalletTransactionDto>> GetTransaction(Guid id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction == null)
            return NotFound();

        return Ok(_mapper.Map<WalletTransactionDto>(transaction));
    }
}
