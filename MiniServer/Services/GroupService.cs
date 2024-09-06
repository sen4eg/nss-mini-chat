using MiniProtoImpl;
using MiniServer.Core;
using MiniServer.Core.Events;
using MiniServer.Data.Model;
using Microsoft.Extensions.DependencyInjection;

namespace MiniServer.Services;

public interface IGroupService
{
    Task CreateGroup(AuthorizedRequest<CreateGroupRequest> authorizedRequest);
    Task DeleteGroup(AuthorizedRequest<DeleteGroupRequest> authorizedRequest);
    Task AddMember(AuthorizedRequest<AddMemberRequest> authorizedRequest);
    Task RemoveMember(AuthorizedRequest<RemoveMemberRequest> authorizedRequest);
    IEnumerable<long> GetGroupMembers(long requestReceiverId);
}

public class GroupService : IGroupService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly EventDispatcher _eventDispatcher;
    private readonly ICommEventFactory _commEventFactory;

    public GroupService(IServiceScopeFactory serviceScopeFactory, EventDispatcher eventDispatcher, ICommEventFactory commEventFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _eventDispatcher = eventDispatcher;
        _commEventFactory = commEventFactory;
    }

    public Task CreateGroup(AuthorizedRequest<CreateGroupRequest> authorizedRequest)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var persistenceService = scope.ServiceProvider.GetRequiredService<IPersistenceService>();

            try
            {
                var members = new HashSet<long>(authorizedRequest.Request.Members.ToHashSet());
                members.Add(authorizedRequest.UserId);

                var grp = new Group(authorizedRequest.Request.Name, authorizedRequest.Request.Description, authorizedRequest.UserId, members);

                persistenceService.SaveAsync(grp);

                var groupCreatedEvent = _commEventFactory.Create<GroupCreatedEvent, Group>(grp, () => { });

                _eventDispatcher.EnqueueEvent(async () =>
                {
                    try
                    {
                        await groupCreatedEvent.Execute(new TaskCompletionSource<Group>());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
            catch (Exception e)
            {
                authorizedRequest.UserConnection.ResponseStream.WriteAsync(new CommunicationResponse()
                {
                    CreateGroup = new CreateGroupResponse
                    {
                        ErrorMsg = "Failed to create group",
                        Id = -32,
                        IsSucceed = false,
                        Name = authorizedRequest.Request.Name
                    }
                });
            }
        }

        return Task.CompletedTask;
    }

    public Task DeleteGroup(AuthorizedRequest<DeleteGroupRequest> authorizedRequest)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var persistenceService = scope.ServiceProvider.GetRequiredService<IPersistenceService>();

            try
            {
                var grp = persistenceService.GetByIdAsync<Group>(authorizedRequest.Request.Id).GetAwaiter().GetResult();
                persistenceService.DeleteAsync(grp);
                // TODO: Notify all users about the group deletion
            }
            catch (Exception e)
            {
                authorizedRequest.UserConnection.ResponseStream.WriteAsync(new CommunicationResponse()
                {
                    DeleteGroup = new DeleteGroupResponse
                    {
                        ErrorMsg = e.Message,
                        Id = -32,
                        IsSucceed = false,
                    }
                });
            }
        }

        return Task.CompletedTask;
    }

    public Task AddMember(AuthorizedRequest<AddMemberRequest> authorizedRequest)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var persistenceService = scope.ServiceProvider.GetRequiredService<IPersistenceService>();

            try
            {
                var grp = persistenceService.GetByIdAsync<Group>(authorizedRequest.Request.GroupId).GetAwaiter().GetResult();
                var member = authorizedRequest.Request.MemberId;
                grp.GroupRoles.Add(new GroupRole(member, grp, GroupRoleTypes.Member));
                persistenceService.UpdateAsync(grp);
                // TODO: Notify all users about the new member
            }
            catch (Exception e)
            {
                authorizedRequest.UserConnection.ResponseStream.WriteAsync(new CommunicationResponse()
                {
                    AddMember = new AddMemberResponse
                    {
                        ErrorMsg = e.Message,
                        IsSucceed = false,
                        Id = authorizedRequest.Request.GroupId
                    }
                });
            }
        }

        return Task.CompletedTask;
    }

    public Task RemoveMember(AuthorizedRequest<RemoveMemberRequest> authorizedRequest)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var persistenceService = scope.ServiceProvider.GetRequiredService<IPersistenceService>();

            try
            {
                var grp = persistenceService.GetByIdAsync<Group>(authorizedRequest.Request.Id).GetAwaiter().GetResult();
                var member = authorizedRequest.Request.MemberId;
                var role = grp.GroupRoles.FirstOrDefault(gr => gr.UserId == member);
                if (role != null)
                {
                    grp.GroupRoles.Remove(role);
                    persistenceService.UpdateAsync(grp);
                }
            }
            catch (Exception e)
            {
                authorizedRequest.UserConnection.ResponseStream.WriteAsync(new CommunicationResponse()
                {
                    RemoveMember = new RemoveMemberResponse
                    {
                        ErrorMsg = e.Message,
                        IsSucceed = false,
                        Id = authorizedRequest.Request.Id,
                        MemberId = authorizedRequest.Request.MemberId
                    }
                });
            }
        }

        return Task.CompletedTask;
    }

    public IEnumerable<long> GetGroupMembers(long requestReceiverId)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var persistenceService = scope.ServiceProvider.GetRequiredService<IPersistenceService>();
            var grp = persistenceService.GetByIdAsync<Group>(Math.Abs(requestReceiverId)).GetAwaiter().GetResult();
            return grp.GroupRoles.Select(gr => gr.UserId);
        }
    }
}
