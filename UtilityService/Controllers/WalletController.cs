using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("balance")]
    public async Task<ActionResult<WalletBalanceDto>> GetBalance(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        var balance = await _walletService.GetBalanceAsync(username, cancellationToken);
        return Ok(balance);
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<PagedTransactionsDto>> GetTransactions(
        [FromQuery] TransactionQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        var result = await _walletService.GetTransactionsAsync(username, queryParams, cancellationToken);
        return Ok(result);
    }

    [HttpGet("transactions/{id:guid}")]
    public async Task<ActionResult<WalletTransactionDto>> GetTransaction(Guid id, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        try
        {
            var transaction = await _walletService.GetTransactionAsync(username, id, cancellationToken);
            return Ok(transaction);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<WalletTransactionDto>> CreateDeposit(
        [FromBody] CreateDepositDto dto,
        CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        try
        {
            var transaction = await _walletService.CreateDepositAsync(username, dto, cancellationToken);
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<WalletTransactionDto>> CreateWithdrawal(
        [FromBody] CreateWithdrawalDto dto,
        CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        try
        {
            var transaction = await _walletService.CreateWithdrawalAsync(username, dto, cancellationToken);
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
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
}
