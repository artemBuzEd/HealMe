using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.AI.Core.DTOs;
using Modules.AI.Core.Interfaces;

namespace Modules.AI.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly IAnamnesisPdfService _pdfService;

    public AiController(IAiService aiService, IAnamnesisPdfService pdfService)
    {
        _aiService = aiService;
        _pdfService = pdfService;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var response = await _aiService.SendMessageAsync(userId, request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var sessions = await _aiService.GetUserSessionsAsync(userId);
        return Ok(sessions);
    }

    [HttpGet("sessions/{id:guid}")]
    public async Task<IActionResult> GetSessionMessages(Guid id)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var messages = await _aiService.GetSessionMessagesAsync(id, userId);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("sessions/{id:guid}/pdf/doctor")]
    public async Task<IActionResult> DownloadDoctorPdf(Guid id)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var data = await _aiService.GetAnamnesisPdfDataAsync(id, userId);
            var pdf = _pdfService.GenerateDoctorPdf(data);
            return File(pdf, "application/pdf", $"anamnesis-report-{id}.pdf");
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("sessions/{id:guid}/pdf/patient")]
    public async Task<IActionResult> DownloadPatientPdf(Guid id)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var data = await _aiService.GetAnamnesisPdfDataAsync(id, userId);
            var pdf = _pdfService.GeneratePatientPdf(data);
            return File(pdf, "application/pdf", $"consultation-summary-{id}.pdf");
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
}
