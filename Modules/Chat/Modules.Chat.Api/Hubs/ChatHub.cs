using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Modules.Appointments.Core.Interfaces;
using Modules.Chat.Core.DTOs;
using Modules.Chat.Core.Interfaces;

namespace Modules.Chat.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IAppointmentService _appointmentService;

    public ChatHub(IChatService chatService, IAppointmentService appointmentService)
    {
        _chatService = chatService;
        _appointmentService = appointmentService;
    }

    public async Task JoinGroup(Guid appointmentId)
    {
        var userId = Context.User?.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) throw new HubException("Unauthorized");
        
        
        var appointment = await _appointmentService.GetAppointmentAuthDetailsAsync(appointmentId);
        if(appointment == null) throw new HubException("Appointment not found");
        
        bool isParticipant = (appointment.DoctorUserId == userId || appointment.PatientUserId == userId);
        
        if (!isParticipant)
            throw new HubException("You are not allowed to join this private chat.");

        await Groups.AddToGroupAsync(Context.ConnectionId, appointmentId.ToString());
    }

    public async Task SendMessage(Guid appointmentId, string message)
    {
        var userId = Context.User?.FindFirstValue("Id");
        if (string.IsNullOrEmpty(userId)) throw new HubException("Unauthorized");

        // Save message
        var savedMessage = await _chatService.SaveMessageAsync(appointmentId, userId, message);

        // Broadcast to group
        await Clients.Group(appointmentId.ToString()).SendAsync("ReceiveMessage", savedMessage);
    }
}
