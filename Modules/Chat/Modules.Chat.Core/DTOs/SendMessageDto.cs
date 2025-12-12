namespace Modules.Chat.Core.DTOs;

public class SendMessageDto
{
    public Guid AppointmentId { get; set; }
    public string Message { get; set; } = string.Empty;
}
