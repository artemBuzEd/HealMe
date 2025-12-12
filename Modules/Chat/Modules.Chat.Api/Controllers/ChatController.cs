using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Chat.Core.Interfaces;
using Modules.Appointments.Core.Interfaces;

namespace Modules.Chat.Api.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IAppointmentService _appointmentService;

    public ChatController(IChatService chatService, IAppointmentService appointmentService)
    {
        _chatService = chatService;
        _appointmentService = appointmentService;
    }

    [HttpGet("{appointmentId:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid appointmentId)
    {
        var userId = User.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();



        try 
        {
            var appointment = await _appointmentService.GetAppointmentAuthDetailsAsync(appointmentId);
            
            bool isParticipant = (appointment.DoctorUserId == userId || appointment.PatientUserId == userId);
            
            if (!isParticipant)
                return Forbid();

            var messages = await _chatService.GetMessagesAsync(appointmentId);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
}
