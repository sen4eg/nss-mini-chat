using Microsoft.EntityFrameworkCore;
using MiniServer.Data.DTO;

namespace MiniServer.Data.Repository; 
public interface IMessageRepository
{
    Task CreateMessageAsync(MessageDTO message);
    Task<IEnumerable<MessageDTO>> GetMessagesAsync(long receiverId, long msgIdOffset, int count);
    Task DeleteMessageAsync(long messageId);
    Task UpdateMessageAsync(MessageDTO message);
    Task<long> GetLastMessageId();
}
public class MessageRepository : IMessageRepository{
    private ChatContext _context;
    public MessageRepository(ChatContext context) {
        _context = context;
    }

    public Task CreateMessageAsync(MessageDTO message) {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<MessageDTO>> GetMessagesAsync(long receiverId, long msgIdOffset, int count) {
        throw new NotImplementedException();
    }

    public Task DeleteMessageAsync(long messageId) {
        throw new NotImplementedException();
    }

    public Task UpdateMessageAsync(MessageDTO message) {
        throw new NotImplementedException();
    }

    public async Task<long> GetLastMessageId()
    {
        var lastMessage = await _context.Messages
            .OrderByDescending(m => m.MessageId)
            .FirstOrDefaultAsync();

        // Return the MessageId if a message is found, otherwise return -1
        return lastMessage?.MessageId ?? -1;
    }
}