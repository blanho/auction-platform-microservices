using Common.Domain.Entities;

namespace PaymentService.Domain.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal HeldAmount { get; set; }
    public decimal AvailableBalance => Balance - HeldAmount;
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
}
