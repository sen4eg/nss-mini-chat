using MiniProtoImpl;
using MiniServer.Core;
using MiniServer.Core.Events;
using MiniServer.Data.Model;

namespace MiniServer.Services; 


public interface IGroupService
{
    Task CreateGroup(AuthorizedRequest<CreateGroupRequest> authorizedRequest);
    Task DeleteGroup(AuthorizedRequest<DeleteGroupRequest> authorizedRequest);
    Task AddMember(AuthorizedRequest<AddMemberRequest> authorizedRequest);
    Task RemoveMember(AuthorizedRequest<RemoveMemberRequest> authorizedRequest);
    IEnumerable<long> GetGroupMembers(long requestReceiverId);
}

public class GroupService: IGroupService{
    readonly IPersistenceService _persistenceService;
    private readonly EventDispatcher _eventDispatcher;
    private readonly ICommEventFactory _commEventFactory;

    public GroupService(IPersistenceService persistenceService, EventDispatcher eventDispatcher, ICommEventFactory commEventFactory) {
        _eventDispatcher = eventDispatcher;
        _commEventFactory = commEventFactory;
        _persistenceService = persistenceService;    
    }
    public Task CreateGroup(AuthorizedRequest<CreateGroupRequest> authorizedRequest) {
        try {
            // Create group
            var members = new HashSet<long>(authorizedRequest.Request.Members.ToHashSet());
            members.Add(authorizedRequest.UserId);
            
            var grp = new Group(authorizedRequest.Request.Name, authorizedRequest.Request.Description, authorizedRequest.UserId, members);
            // Save group to database
            _persistenceService.SaveAsync(grp);
            
            // Send group to all members 
            var groupCreatedEvent = _commEventFactory.Create<GroupCreatedEvent, Group>(grp, () => { });

            _eventDispatcher.EnqueueEvent(async () => {
                try {
                    await groupCreatedEvent.Execute(new TaskCompletionSource<Group>());
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            });
            
        } catch (Exception e) {
            // throw new Exception("Failed to create group", e)
            authorizedRequest.UserConnection.ResponseStream.WriteAsync(new CommunicationResponse() {
                CreateGroup = new CreateGroupResponse {
                    ErrorMsg = "Failed to create group",
                    Id = -32,
                    IsSucceed = false,
                    Name = authorizedRequest.Request.Name
                }
            });
        }
        
        return Task.CompletedTask;
    }
    

    public Task DeleteGroup(AuthorizedRequest<DeleteGroupRequest> authorizedRequest) {
        try {
            // Get group
            var grp = _persistenceService.GetByIdAsync<Group>(authorizedRequest.Request.Id).GetAwaiter().GetResult();
            // Delete group
            _persistenceService.DeleteAsync(grp);
            // Send group to all members ??
            
            // TODO EVENT MAKE ALL USERS KNOW ABOUT THE GROUP DELETION
        } catch (Exception e) {
            // throw new Exception("Failed to delete group", e)
            authorizedRequest.UserConnection.ResponseStream.WriteAsync(new CommunicationResponse() {
                DeleteGroup = new DeleteGroupResponse {
                    ErrorMsg = e.Message,
                    Id = -32,
                    IsSucceed = false,
                }
            });
        }
        
        return Task.CompletedTask;

    }

    public Task AddMember(AuthorizedRequest<AddMemberRequest> authorizedRequest) {
        try {
            var grp = _persistenceService.GetByIdAsync<Group>(authorizedRequest.Request.GroupId).GetAwaiter().GetResult();
            var member = authorizedRequest.Request.MemberId;
            grp.GroupRoles.Add(new GroupRole(member, grp, GroupRoleTypes.Member));
            _persistenceService.UpdateAsync(grp);
            // TODO EVENT MAKE ALL USERS KNOW ABOUT THE NEW MEMBER
        }
        catch (Exception e) {
            Console.WriteLine(e);
            authorizedRequest.UserConnection.ResponseStream.WriteAsync(new CommunicationResponse() {
                AddMember = new AddMemberResponse {
                    ErrorMsg = e.Message,
                    IsSucceed = false,
                    Id = authorizedRequest.Request.GroupId
                }
            });
        }
        return Task.CompletedTask;
    }

    public Task RemoveMember(AuthorizedRequest<RemoveMemberRequest> authorizedRequest) {
        try {
            var grp = _persistenceService.GetByIdAsync<Group>(authorizedRequest.Request.Id).GetAwaiter().GetResult();
            var member = authorizedRequest.Request.MemberId;
            var role = grp.GroupRoles.FirstOrDefault(gr => gr.UserId == member);
            if (role != null) {
                grp.GroupRoles.Remove(role);
                _persistenceService.UpdateAsync(grp);
            }
        }
        catch (Exception e) {
            authorizedRequest.UserConnection.ResponseStream.WriteAsync(new CommunicationResponse() {
                RemoveMember = new RemoveMemberResponse {
                    ErrorMsg = e.Message,
                    IsSucceed = false,
                    Id = authorizedRequest.Request.Id,
                    MemberId = authorizedRequest.Request.MemberId
                }
            });
        }
        return Task.CompletedTask;
    }

    public IEnumerable<long> GetGroupMembers(long requestReceiverId) {
        var grp = _persistenceService.GetByIdAsync<Group>(Int64.Abs(requestReceiverId)).GetAwaiter().GetResult();
        return grp.GroupRoles.Select(gr => gr.UserId);
    }
}