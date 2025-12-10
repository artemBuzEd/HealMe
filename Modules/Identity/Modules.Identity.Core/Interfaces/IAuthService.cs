using Modules.Identity.Core.Entities;
using Modules.Identity.Core.Entities.Enums;
using Modules.Identity.Core.DTOs;

namespace Modules.Identity.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName, Gender gender, string roleName);
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> ForgotPasswordAsync(string email);
    Task<AuthResult> ResetPasswordAsync(string email, string token, string newPassword);
    Task<AuthResult> RefreshTokenAsync(TokenRequest request);
}