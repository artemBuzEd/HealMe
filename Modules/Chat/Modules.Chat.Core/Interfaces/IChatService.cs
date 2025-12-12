using Modules.Chat.Core.DTOs;

namespace Modules.Chat.Core.Interfaces;

public interface IChatService
{
    Task<ChatMessageDto> SaveMessageAsync(Guid appointmentId, string senderId, string message);
    Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(Guid appointmentId);
}
