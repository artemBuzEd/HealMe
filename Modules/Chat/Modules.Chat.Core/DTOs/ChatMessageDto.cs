namespace Modules.Chat.Core.DTOs;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
