using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Modules.Identity.Core.DTOs;
using Modules.Identity.Core.Entities;
using Modules.Identity.Core.Entities.Enums;
using Modules.Identity.Core.Interfaces;

namespace Modules.Identity.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<User> userManager, RoleManager<Role> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName, Gender gender, string roleName)
    {
        var userExists = await _userManager.FindByEmailAsync(email);
        if (userExists != null)
            return new AuthResult { Success = false, Message = "User already exists" };

        var user = new User
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            Gender = gender,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return new AuthResult { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

        if (!await _roleManager.RoleExistsAsync(roleName))
            await _roleManager.CreateAsync(new Role { Name = roleName, CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow });

        await _userManager.AddToRoleAsync(user, roleName);

        return await GenerateJwtToken(user);
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return new AuthResult { Success = false, Message = "Invalid credentials" };

        if (!await _userManager.CheckPasswordAsync(user, password))
            return new AuthResult { Success = false, Message = "Invalid credentials" };

        return await GenerateJwtToken(user);
    }

    public async Task<AuthResult> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return new AuthResult { Success = false, Message = "User not found" };

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // In a real we need to  send email, but for now we will return token for testing and development.
        return new AuthResult { Success = true, Message = "Password reset token generated", Token = token };
    }

    public async Task<AuthResult> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return new AuthResult { Success = false, Message = "User not found" };

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return new AuthResult { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

        return new AuthResult { Success = true, Message = "Password reset successfully" };
    }

    private async Task<AuthResult> GenerateJwtToken(User user)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtConfig:Secret"] ?? "ThisIsASecretKeyForJwtTokenGeneration123456"); // Fallback key

        var claims = new List<Claim>
        {
            new Claim("Id", user.Id),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(6),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        return new AuthResult
        {
            Success = true,
            Token = jwtToken,
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                Roles = userRoles
            }
        };
    }
}
