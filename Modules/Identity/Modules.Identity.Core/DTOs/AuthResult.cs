namespace Modules.Identity.Core.DTOs;

public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public UserInfo? User { get; set; }
}