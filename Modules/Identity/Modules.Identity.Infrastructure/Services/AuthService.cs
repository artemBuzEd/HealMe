using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return new AuthResult { Success = false, Message = "Invalid credentials" };

        if (!await _userManager.CheckPasswordAsync(user, password))
            return new AuthResult { Success = false, Message = "Invalid credentials" };

        return await GenerateTokensAsync(user);
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

    public async Task<AuthResult> RefreshTokenAsync(TokenRequest request)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtConfig:Secret"] ?? "ThisIsASecretKeyForJwtTokenGeneration123456");

        try
        {
            // Validation 1: Validate JWT token format
            var tokenVerification = jwtTokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false, // Ignore expiry
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            // Validation 2: Validate encryption algorithm
            if (validatedToken is JwtSecurityToken jwtSecurityToken && !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return new AuthResult { Success = false, Message = "Invalid token" };
            }

            // Validation 3: Validate expiry date
            var utcExpiryDate = long.Parse(tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var expiryDate = DateTimeOffset.FromUnixTimeSeconds(utcExpiryDate).UtcDateTime;

            if (expiryDate > DateTime.UtcNow)
            {
                return new AuthResult { Success = false, Message = "Token has not expired yet" };
            }

            // Validation 4: Validate existence of the token
            var storedToken = await _userManager.FindByIdAsync(tokenVerification.Claims.FirstOrDefault(x => x.Type == "Id").Value);

            if (storedToken == null || storedToken.RefreshToken != request.RefreshToken)
            {
                return new AuthResult { Success = false, Message = "Invalid refresh token" };
            }

            // Validation 5: Validate if used
            if (storedToken.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new AuthResult { Success = false, Message = "Refresh token has expired" };
            }

            return await GenerateTokensAsync(storedToken);
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, Message = "Server error" };
        }
    }

    private async Task<AuthResult> GenerateTokensAsync(User user)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtConfig:Secret"] ?? "ThisIsASecretKeyForJwtTokenGeneration123456");

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
            Expires = DateTime.UtcNow.AddMinutes(15), // Short-lived access token
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Long-lived refresh token
        await _userManager.UpdateAsync(user);

        return new AuthResult
        {
            Success = true,
            Token = jwtToken,
            RefreshToken = refreshToken,
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

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
