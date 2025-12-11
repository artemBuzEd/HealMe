using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Doctors.Core.Interfaces;
using Modules.Patients.Core.Interfaces;
using Modules.Identity.Core.DTOs;
using Modules.Identity.Core.Interfaces;

namespace Modules.Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;

    public AuthController(IAuthService authService, IDoctorService doctorService, IPatientService patientService)
    {
        _authService = authService;
        _doctorService = doctorService;
        _patientService = patientService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Gender,
            request.IsDoctor ? "Doctor" : "User"
        );

        if (!result.Success)
            return BadRequest(result.Message);

        if (request.IsDoctor && result.User != null)
        {
            await _doctorService.CreateProfileAsync(result.User.Id, result.User.FirstName, result.User.LastName);
        }
        else if (result.User != null)
        {
            await _patientService.CreateProfileAsync(result.User.Id, result.User.FirstName, result.User.LastName, result.User.Email, result.User.Gender);
        }

        return Ok(result);
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (!result.Success)
            return Unauthorized(result.Message);

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request.Email);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result);
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result);
    }
}
