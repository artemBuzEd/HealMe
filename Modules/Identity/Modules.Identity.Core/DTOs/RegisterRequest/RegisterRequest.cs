using System.ComponentModel.DataAnnotations;
using Modules.Identity.Core.Entities.Enums;

namespace Modules.Identity.Core.DTOs;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public Gender Gender { get; set; }
    
    public bool IsDoctor { get; set; }
}
