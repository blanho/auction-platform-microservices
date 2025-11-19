namespace Identity.Api.DTOs.Seller;

public class SellerStatusResponse
{
    public bool IsSeller { get; set; }
    public bool CanBecomeSeller { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class BecomeSellerRequest
{
    public bool AcceptTerms { get; set; }
}
