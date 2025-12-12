namespace Modules.Chat.Core.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
