using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Controllers;

[ApiController]
[Route("api/v1/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class PaymentsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public PaymentsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("withdrawals/pending")]
    public async Task<ActionResult<List<AdminWithdrawalDto>>> GetPendingWithdrawals(CancellationToken cancellationToken)
    {
        var withdrawals = await _walletService.GetPendingWithdrawalsAsync(cancellationToken);
        return Ok(withdrawals);
    }

    [HttpPost("withdrawals/{id:guid}/approve")]
    public async Task<IActionResult> ApproveWithdrawal(
        Guid id,
        [FromBody] ProcessWithdrawalDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await _walletService.ApproveWithdrawalAsync(id, dto.ExternalTransactionId, cancellationToken);
            return Ok(new { message = "Withdrawal approved successfully." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("withdrawals/{id:guid}/reject")]
    public async Task<IActionResult> RejectWithdrawal(
        Guid id,
        [FromBody] RejectWithdrawalDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await _walletService.RejectWithdrawalAsync(id, dto.Reason, cancellationToken);
            return Ok(new { message = "Withdrawal rejected." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<PaymentStatisticsDto>> GetStatistics(CancellationToken cancellationToken)
    {
        var statistics = await _walletService.GetStatisticsAsync(cancellationToken);
        return Ok(statistics);
    }
}
