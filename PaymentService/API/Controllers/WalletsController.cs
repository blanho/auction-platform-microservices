using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Commands.CreateWallet;
using PaymentService.Application.Commands.Deposit;
using PaymentService.Application.Commands.Withdraw;
using PaymentService.Application.DTOs;
using PaymentService.Application.Queries.GetTransactionById;
using PaymentService.Application.Queries.GetWallet;
using PaymentService.Application.Queries.GetWalletTransactions;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WalletsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WalletsController> _logger;

    public WalletsController(
        IMediator mediator,
        ILogger<WalletsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<WalletDto>> GetWallet(string username, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWalletQuery { Username = username }, cancellationToken);
        
        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        if (result.Value == null)
            return NotFound();

        return Ok(result.Value);
    }

    [HttpPost("{username}/create")]
    public async Task<ActionResult<WalletDto>> CreateWallet(string username, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        var result = await _mediator.Send(new CreateWalletCommand
        {
            UserId = userId,
            Username = username
        }, cancellationToken);

        if (!result.IsSuccess)
            return Conflict(ProblemDetailsHelper.FromError(result.Error!));

        return CreatedAtAction(nameof(GetWallet), new { username }, result.Value);
    }

    [HttpPost("{username}/deposit")]
    public async Task<ActionResult<WalletTransactionDto>> Deposit(
        string username,
        [FromBody] DepositDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DepositCommand
        {
            Username = username,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Description = dto.Description
        }, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "Wallet.NotFound")
                return NotFound(ProblemDetailsHelper.FromError(result.Error));
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Ok(result.Value);
    }

    [HttpPost("{username}/withdraw")]
    public async Task<ActionResult<WalletTransactionDto>> Withdraw(
        string username,
        [FromBody] WithdrawDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new WithdrawCommand
        {
            Username = username,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Description = dto.Description
        }, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "Wallet.NotFound")
                return NotFound(ProblemDetailsHelper.FromError(result.Error));
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Ok(result.Value);
    }

    [HttpGet("{username}/transactions")]
    public async Task<ActionResult<IEnumerable<WalletTransactionDto>>> GetTransactions(
        string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetWalletTransactionsQuery
        {
            Username = username,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
        Response.Headers.Append("X-Page", page.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        return Ok(result.Value.Transactions);
    }

    [HttpGet("transactions/{id:guid}")]
    public async Task<ActionResult<WalletTransactionDto>> GetTransaction(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTransactionByIdQuery { TransactionId = id }, cancellationToken);
        
        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        if (result.Value == null)
            return NotFound();

        return Ok(result.Value);
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
