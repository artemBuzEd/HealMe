using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Appointments.Core.DTOs;
using Modules.Appointments.Core.Interfaces;

namespace Modules.Appointments.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (User.IsInRole("Doctor")) return BadRequest("Doctors cannot book appointments");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var appointment = await _appointmentService.BookAppointmentAsync(userId, request);
            return Ok(appointment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("my-appointments")]
    public async Task<IActionResult> GetMyAppointments()
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            if (User.IsInRole("Doctor"))
            {
                var appointments = await _appointmentService.GetDoctorAppointmentsAsync(userId);
                return Ok(appointments);
            }
            else
            {
                var appointments = await _appointmentService.GetPatientAppointmentsAsync(userId);
                return Ok(appointments);
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Policy = "DoctorPolicy")]
    [HttpPut("{id:guid}/confirm")]
    public async Task<IActionResult> ConfirmAppointment(Guid id)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var appointment = await _appointmentService.ConfirmAppointmentAsync(userId, id);
            return Ok(appointment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelAppointment(Guid id)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            await _appointmentService.CancelAppointmentAsync(userId, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
