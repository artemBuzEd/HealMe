using Microsoft.EntityFrameworkCore;
using Modules.Chat.Core.DTOs;
using Modules.Chat.Core.Entities;
using Modules.Chat.Core.Interfaces;
using Modules.Chat.Infrastructure.Persistence;

namespace Modules.Chat.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly ChatDbContext _dbContext;

    public ChatService(ChatDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChatMessageDto> SaveMessageAsync(Guid appointmentId, string senderId, string message)
    {
        var chatMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointmentId,
            SenderId = senderId,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        await _dbContext.AddAsync(chatMessage);
        await _dbContext.SaveChangesAsync();

        return new ChatMessageDto
        {
            Id = chatMessage.Id,
            AppointmentId = chatMessage.AppointmentId,
            SenderId = chatMessage.SenderId,
            Message = chatMessage.Message,
            Timestamp = chatMessage.Timestamp
        };
    }

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(Guid appointmentId)
    {
        var messages = await _dbContext.Set<ChatMessage>()
            .Where(x => x.AppointmentId == appointmentId)
            .OrderBy(x => x.Timestamp)
            .ToListAsync();

        return messages.Select(x => new ChatMessageDto
        {
            Id = x.Id,
            AppointmentId = x.AppointmentId,
            SenderId = x.SenderId,
            Message = x.Message,
            Timestamp = x.Timestamp
        });
    }
}
