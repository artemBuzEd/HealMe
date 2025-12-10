using System.ComponentModel.DataAnnotations;

namespace Modules.Identity.Core.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
