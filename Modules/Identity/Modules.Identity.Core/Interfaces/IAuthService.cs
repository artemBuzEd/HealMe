using Modules.Identity.Core.Entities;
using Modules.Identity.Core.Entities.Enums;

namespace Modules.Identity.Core.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message, User? User)> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        Gender gender,
        string roleName);
    
    Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password);
    
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> UserExistsAsync(string email);
}