using Microsoft.EntityFrameworkCore;
using MiniProtoImpl;
using MiniServer.Data.DTO;

namespace MiniServer.Data.Repository; 
public interface IMessageRepository
{
    Task CreateMessageAsync(MessageDTO message);
    Task<IEnumerable<MessageDTO>> GetMessagesAsync(long receiverId, long msgIdOffset, int count);
    Task DeleteMessageAsync(long messageId, long userId);
    Task UpdateMessageAsync(MessageDTO message);
    Task<long> GetLastMessageId();
    List<DialogStruct> GetDialogsForUser(long authorizedRequestUserId, long requestLastMessageId);
    DialogBodyStruct GetMessagesForUser(long authorizedRequestUserId, RequestDialog authorizedRequestRequest);
}

public struct DialogStruct {
    public MessageDTO LastMessage;
    public long unreadCount;
    public long DialogId;
    public DialogStruct(long dialogId, MessageDTO lastMessage, long unreadCount) {
        DialogId = dialogId;
        LastMessage = lastMessage;
        this.unreadCount = unreadCount;
    }
}

public struct DialogBodyStruct {
    public List<MessageDTO> messages;
    public long lastMessageId;
    public long DialogId;
    public DialogBodyStruct(long dialogId, long lastMessageId, List<MessageDTO> messages) {
        DialogId = dialogId;
        this.lastMessageId = lastMessageId;
        this.messages = messages;
    }
}
public class MessageRepository : IMessageRepository{
    private ChatContext _context;
    public MessageRepository(ChatContext context) {
        _context = context;
    }

    public Task CreateMessageAsync(MessageDTO message) {
        var messageEntity = message.ConvertToDbModel();
        _context.Messages.Add(messageEntity);
        return _context.SaveChangesAsync();
    }

    public Task<IEnumerable<MessageDTO>> GetMessagesAsync(long receiverId, long msgIdOffset, int count) {
        throw new NotImplementedException();
    }

    public Task UpdateMessageAsync(MessageDTO message) {
        var messageToUpdate = _context.Messages.FirstOrDefault(m => m.MessageId == message.TargetId);
        if (messageToUpdate != null && messageToUpdate.UserId == message.UserId)
        {
            messageToUpdate.EditFromDTO(message);
        }
        return _context.SaveChangesAsync();
    }

    public Task DeleteMessageAsync(long messageId, long userId) {
        var message = _context.Messages.FirstOrDefault(m => m.MessageId == messageId);
        if (message == null || message.UserId != userId)
        {
            return Task.CompletedTask;
        }
        message.isDeleted = true;
        return _context.SaveChangesAsync();
    }

    public async Task<long> GetLastMessageId()
    {
        var lastMessage = await _context.Messages
            .OrderByDescending(m => m.MessageId)
            .FirstOrDefaultAsync();

        // Return the MessageId if a message is found, otherwise return -1
        return lastMessage?.MessageId ?? -1;
    }

    public List<DialogStruct> GetDialogsForUser(long authorizedRequestUserId, long requestLastMessageId) {
        var result = _context.Messages
            .Where(m => m.ReceiverId == authorizedRequestUserId && m.MessageId >= requestLastMessageId && !m.isDeleted)
            .OrderBy(m => m.MessageId)
            .GroupBy(m => m.UserId)
            .Select(g => new DialogStruct(
                g.Key,
                new MessageDTO(g.OrderByDescending(m => m.MessageId).FirstOrDefault()),
                g.Count()
            ))
            .ToList();

        return result;
    }           

    public DialogBodyStruct GetMessagesForUser(long authorizedRequestUserId, RequestDialog authorizedRequestRequest)
    {
        var list = _context.Messages
            .Where(m => m.UserId == authorizedRequestUserId || m.UserId == authorizedRequestRequest.DialogId)
            .Where(m => m.ReceiverId == authorizedRequestUserId || m.ReceiverId == authorizedRequestRequest.DialogId)
            .Where(m => m.MessageId <= authorizedRequestRequest.LastMessageId && !m.isDeleted)
            .OrderByDescending(m => m.Timestamp)
            .Skip(authorizedRequestRequest.Offset)
            .Take(authorizedRequestRequest.Count)
            .Select(m => new MessageDTO(m))
            .ToList();
        
        if (list.Count == 0)
        {
            return new DialogBodyStruct(authorizedRequestRequest.DialogId, authorizedRequestRequest.LastMessageId, list);
        }

        var lastMessage = list.Last(); // This assumes list has at least one element

        var result = new DialogBodyStruct(authorizedRequestRequest.DialogId, lastMessage.MessageId, list);
        return result;
    }
}