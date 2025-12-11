using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Patients.Core.DTOs;
using Modules.Patients.Core.Interfaces;

namespace Modules.Patients.Api.Controllers;

[ApiController]
[Route("api/patients")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var profile = await _patientService.GetProfileAsync(userId);
        if (profile == null) return NotFound("Patient profile not found");

        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdatePatientProfileRequest request)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var profile = await _patientService.UpdateProfileAsync(userId, request);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
