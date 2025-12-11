using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Doctors.Core.DTOs;
using Modules.Doctors.Core.Interfaces;

namespace Modules.Doctors.Api.Controllers;

[ApiController]
[Route("api/doctors")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }
    
    [Authorize(Policy = "DoctorPolicy")]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var profile = await _doctorService.GetProfileAsync(userId);
        if (profile == null) return NotFound("Doctor profile not found");

        return Ok(profile);
    }
    [Authorize(Policy = "DoctorPolicy")]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateDoctorProfileRequest request)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var profile = await _doctorService.UpdateProfileAsync(userId, request);
        return Ok(profile);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetDoctors()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        return Ok(doctors);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDoctorById(Guid id)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id);
        if (doctor == null) return NotFound("Doctor not found");
        return Ok(doctor);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/availability")]
    public async Task<IActionResult> GetDoctorAvailability(Guid id)
    {
        var availability = await _doctorService.GetAvailabilityAsync(id);
        return Ok(availability);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/reviews")]
    public async Task<IActionResult> GetDoctorReviews(Guid id)
    {
        var reviews = await _doctorService.GetReviewsAsync(id);
        return Ok(reviews);
    }
    
    [Authorize(Policy = "DoctorPolicy")]
    [HttpPost("me/availability")]
    public async Task<IActionResult> CreateAvailability([FromBody] CreateAvailabilityRequest request)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var availability = await _doctorService.AddAvailabilityAsync(userId, request);
            return Ok(availability);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Policy = "DoctorPolicy")]
    [HttpPut("me/availability/{id:guid}")]
    public async Task<IActionResult> UpdateAvailability(Guid id, [FromBody] UpdateAvailabilityRequest request)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var availability = await _doctorService.UpdateAvailabilityAsync(userId, id, request);
            return Ok(availability);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Policy = "DoctorPolicy")]
    [HttpDelete("me/availability/{id:guid}")]
    public async Task<IActionResult> DeleteAvailability(Guid id)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            await _doctorService.DeleteAvailabilityAsync(userId, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
