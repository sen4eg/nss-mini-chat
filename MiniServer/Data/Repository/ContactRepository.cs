using MiniProtoImpl;
using MiniServer.Data.DTO;
using MiniServer.Data.Model;
using Message = MiniServer.Data.Model.Message;

namespace MiniServer.Data.Repository; 

public interface IContactRepository {
    Task AddOrUpdate(Contact contact);
    List<Dialog> GetDialogsForUser(long authorizedRequestUserId);
}

public class ContactRepository : IContactRepository {
    private ChatContext _context;

    public ContactRepository(ChatContext context) {
        _context = context;
    }

    public async Task AddOrUpdate(Contact contact) {
        var existingContact = _context.Contacts.FirstOrDefault(c => c.UserId == contact.UserId && c.ContactId == contact.ContactId);
        
        if (existingContact != null) {
        } else {
            _context.Contacts.Add(contact);
            var OppositeContact = new Contact {
                UserId = contact.ContactId,
                ContactId = contact.UserId,
                ContactTypeId = 0,

            };
            _context.Contacts.Add(OppositeContact);
        }
        await _context.SaveChangesAsync();
    }

    public List<Dialog> GetDialogsForUser(long authorizedRequestUserId) {
        var contacts = _context.Contacts.Where(c => c.UserId == authorizedRequestUserId)
            .Select(c => new Dialog {
            ContactId = c.ContactId,
            ContactName = c.User.Username,
            LastMessage = new MessageDTO(
                _context.Messages.Where(m => (m.UserId == authorizedRequestUserId || m.UserId == c.ContactId) && (m.MessageType == 0))
                    .OrderByDescending(m => m.Timestamp)
                    .FirstOrDefault()
                ).ConvertToGrpcMessage(),
        }).ToList();
        return contacts;
    }
}