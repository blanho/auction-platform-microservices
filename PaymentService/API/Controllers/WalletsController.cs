using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly IMapper _mapper;
    private readonly ILogger<WalletsController> _logger;

    public WalletsController(
        IWalletService walletService,
        IMapper mapper,
        ILogger<WalletsController> logger)
    {
        _walletService = walletService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<WalletDto>> GetWallet(string username, CancellationToken cancellationToken)
    {
        var wallet = await _walletService.GetWalletAsync(username, cancellationToken);
        if (wallet == null)
            return NotFound();

        return Ok(wallet);
    }

    [HttpPost("{username}/create")]
    public async Task<ActionResult<WalletDto>> CreateWallet(string username, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            var wallet = await _walletService.CreateWalletAsync(userId, username, cancellationToken);
            return CreatedAtAction(nameof(GetWallet), new { username }, wallet);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{username}/deposit")]
    public async Task<ActionResult<WalletTransactionDto>> Deposit(string username, [FromBody] DepositDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _walletService.DepositAsync(username, dto.Amount, dto.PaymentMethod, dto.Description, cancellationToken);
            return Ok(transaction);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Wallet not found");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{username}/withdraw")]
    public async Task<ActionResult<WalletTransactionDto>> Withdraw(string username, [FromBody] WithdrawDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _walletService.WithdrawAsync(username, dto.Amount, dto.PaymentMethod, dto.Description, cancellationToken);
            return Ok(transaction);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Wallet not found");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{username}/transactions")]
    public async Task<ActionResult<IEnumerable<WalletTransactionDto>>> GetTransactions(
        string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (transactions, totalCount) = await _walletService.GetTransactionsAsync(username, page, pageSize, cancellationToken);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        Response.Headers.Append("X-Page", page.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        return Ok(transactions);
    }

    [HttpGet("transactions/{id:guid}")]
    public async Task<ActionResult<WalletTransactionDto>> GetTransaction(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _walletService.GetTransactionByIdAsync(id, cancellationToken);
        if (transaction == null)
            return NotFound();

        return Ok(transaction);
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
