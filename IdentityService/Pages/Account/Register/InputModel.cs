using System.ComponentModel.DataAnnotations;

namespace IdentityService.Pages.Register;

public class InputModel
{
    [Required]
    public string? Username { get; set; }
    
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    
    [Required]
    public string? Password { get; set; }
    
    [Required]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }
    
    public string? ReturnUrl { get; set; }
    public string? Button { get; set; }
}
