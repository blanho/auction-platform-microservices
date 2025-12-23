using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Commands.CreateWallet;

public record CreateWalletCommand : ICommand<WalletDto>
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
}
