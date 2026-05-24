using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Appointments.Core.DTOs;
using Modules.Appointments.Core.Entities;
using Modules.Appointments.Core.Interfaces;
using Modules.Doctors.Core.DTOs;
using Modules.Doctors.Core.Interfaces;
using Modules.Patients.Core.Interfaces;

namespace Modules.Appointments.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;

    public AppointmentsController(IAppointmentService appointmentService, IDoctorService doctorService, IPatientService patientService)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientService = patientService;
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

    [Authorize(Policy = "DoctorPolicy")]
    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> CompleteAppointment(Guid id)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var appointment = await _appointmentService.CompleteAppointmentAsync(userId, id);
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

    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> CreateReview(Guid id, [FromBody] CreateReviewRequest request)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (User.IsInRole("Doctor")) return BadRequest("Doctors cannot leave reviews");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

            bool isCompleted = appointment.Status == AppointmentStatus.Completed;
            bool isConfirmedAndPast = appointment.Status == AppointmentStatus.Confirmed
                                     && appointment.EndTime < DateTime.UtcNow;

            if (!isCompleted && !isConfirmedAndPast)
                return BadRequest("Reviews can only be left for completed appointments");

            var patient = await _patientService.GetProfileAsync(userId);
            if (patient == null) return BadRequest("Patient profile not found");

            if (appointment.PatientId != patient.Id)
                return Forbid();

            var review = await _doctorService.CreateReviewAsync(
                id, appointment.DoctorId, userId,
                patient.FirstName, patient.LastName, request);

            return Ok(review);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}/review")]
    public async Task<IActionResult> UpdateReview(Guid id, [FromBody] UpdateReviewRequest request)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var existingReview = await _doctorService.GetReviewByAppointmentAsync(id);
            if (existingReview == null) return NotFound("No review found for this appointment");

            var review = await _doctorService.UpdateReviewAsync(existingReview.Id, userId, request);
            return Ok(review);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:guid}/review")]
    public async Task<IActionResult> GetReview(Guid id)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var review = await _doctorService.GetReviewByAppointmentAsync(id);
        if (review == null) return NotFound("No review found for this appointment");

        return Ok(review);
    }
}
