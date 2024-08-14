using MiniServer.Data.Model;

namespace MiniServer.Data.Repository; 
public interface IGroupRepository {
    Task<Group?> GetGroup(long requestReceiverId);
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
}