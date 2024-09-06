using MiniProtoImpl;
using MiniServer.Data.DTO;
using MiniServer.Data.Model;

namespace MiniServer.Data.Repository; 
public interface IGroupRepository {
    Task<Group?> GetGroup(long requestReceiverId);
    IEnumerable<Dialog> GetGroupsForUser(long authorizedRequestUserId);
}

public class GroupRepository : IGroupRepository {
    private readonly ChatContext _context;

    public GroupRepository(ChatContext context)
    {
        _context = context;
    }

    public async Task<Group?> GetGroup(long requestReceiverId) {
        return await _context.Groups.FindAsync(requestReceiverId);
    }

    public IEnumerable<Dialog> GetGroupsForUser(long authorizedRequestUserId) {
        return _context.GroupRoles.Where(gr => gr.UserId == authorizedRequestUserId)
            .Select(gr => new Dialog {
                ContactId = -gr.GroupId,
                ContactName = gr.Group != null? gr.Group.Name:"Unknown Group",
                LastMessage = new MessageDTO(
                    _context.Messages.Where(m => m.ReceiverId == gr.GroupId && m.MessageType == 0)
                        .OrderByDescending(m => m.Timestamp)
                        .FirstOrDefault()
                ).ConvertToGrpcMessage(),
            }).ToList();
    }
}