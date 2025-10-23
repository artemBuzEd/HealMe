using System.Security.Claims;
using Modules.Identity.Core.Entities;

namespace Modules.Identity.Core.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user, IList<string> roles);
    ClaimsPrincipal? ValidateToken(string token);
    string GetUserIdFromToken(string token);
    IList<string> GetRolesFromToken(string token);
}