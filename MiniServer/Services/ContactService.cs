using MiniProtoImpl;
using MiniServer.Data;
using MiniServer.Data.DTO;
using MiniServer.Data.Model;
using MiniServer.Data.Repository;

namespace MiniServer.Services; 
public interface IContactService {
    List<Dialog> GetDialogsForUser(long authorizedRequestUserId);
    void UpdateDialog(long msgUserId, long msgReceiverId);
}

public class ContactService : IContactService {
    private readonly IContactRepository _contactRepository;
    private readonly IGenericRepository<GroupRole> _groupRepository;
    private readonly ChatContext _context;
    
    public ContactService(IContactRepository contactRepository, ChatContext context) {
        _contactRepository = contactRepository;
        _groupRepository = new GenericRepository<GroupRole>(context);
        _context = context;
    }

    public List<Dialog> GetDialogsForUser(long authorizedRequestUserId) {
        var contacts = _contactRepository.GetDialogsForUser(authorizedRequestUserId);
        
        // TODO move this to group repository, MultipleRoles can cause issues!
        // TODO fix unread count isn't implemented
        var groups = _context.GroupRoles.Where(gr => gr.UserId == authorizedRequestUserId)
            .Select(gr => new Dialog {
            ContactId = -gr.GroupId,
            ContactName = gr.Group != null? gr.Group.Name:"Unknown Group",
            LastMessage = new MessageDTO(
                _context.Messages.Where(m => m.ReceiverId == gr.GroupId && m.MessageType == 0)
                    .OrderByDescending(m => m.Timestamp)
                    .FirstOrDefault()
            ).ConvertToGrpcMessage(),
        }).ToList();
        contacts.AddRange(groups);
        return contacts;
    }

    public void UpdateDialog(long msgUserId, long msgReceiverId) {
        _contactRepository.AddOrUpdate(new Contact { UserId = msgUserId, ContactId = msgReceiverId, ContactTypeId = 0});
    }
}