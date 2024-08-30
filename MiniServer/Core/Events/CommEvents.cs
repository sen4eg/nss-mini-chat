using MiniProtoImpl;
using MiniServer.Data.DTO;
using MiniServer.Data.Model;
using MiniServer.Services;
using Message = MiniProtoImpl.Message;

namespace MiniServer.Core.Events;
public class RegisterEvent : EventBase<RegisterResponse>
{
    private readonly RegisterRequest _request;
    private readonly IConnectionLogicService _connectionLogicService;

    public RegisterEvent(RegisterRequest request, IConnectionLogicService connectionLogicService, Action sideEffect)
        : base(sideEffect)
    {
        _request = request;
        _connectionLogicService = connectionLogicService;
    }

    protected override async Task<RegisterResponse> ExecuteAsync()
    {
        // Process the registration logic
        return await _connectionLogicService.RegisterAsync(_request);
    }
}

public class ConnectEvent : EventBase<ConnectResponse> {
    private readonly ConnectRequest _request;
        // any way it can be injected?
    private readonly IConnectionLogicService _connectionLogicService;

    public ConnectEvent(ConnectRequest request, IConnectionLogicService connectionLogicService, Action action) : base(action) {
        this._request = request;
        this._connectionLogicService = connectionLogicService;
    }

    protected override async Task<ConnectResponse> ExecuteAsync() {
        return await _connectionLogicService.ConnectAsync(_request);
    }
}

public class RefreshTokenEvent : EventBase<ConnectResponse> {
    private readonly RefreshTokenRequest _request;
    private readonly IAuthenticationService _authenticationService;

    public RefreshTokenEvent(RefreshTokenRequest request, IAuthenticationService authenticationService, Action action) : base(action) {
        _request = request;
        _authenticationService = authenticationService;
    }

    protected override async Task<ConnectResponse> ExecuteAsync() {
        return await _authenticationService.RefreshTokenAsync(_request);
    }
}
public class UserConnectedEvent : EventBase<UserConnection> {
    private readonly UserConnection _userConnection;
    private readonly IConnectionManager _connectionManager;
    private readonly IAuthenticationService _authenticationService;

    public UserConnectedEvent(UserConnection userConnection, IConnectionManager connectionManager, IAuthenticationService authenticationService, Action sideEffect) : base(sideEffect) {
        _userConnection = userConnection;
        _connectionManager = connectionManager;
        _authenticationService = authenticationService;
    }
    protected override Task<UserConnection> ExecuteAsync() {
        _connectionManager.HandleConnectedUser(_userConnection);
        
        return new Task<UserConnection>(() => _userConnection);
    }
}

public class MessageSentEvent : EventBase<AuthorizedRequest<Message>> {
    private readonly AuthorizedRequest<Message> _message;
    private readonly IConnectionManager _connectionManager;
    private readonly IMessagingService _messagingService;

    public MessageSentEvent(AuthorizedRequest<Message> message, IConnectionManager connectionManager, IMessagingService messagingService, Action sideEffect) : base(sideEffect) {
        _message = message;
        _connectionManager = connectionManager;
        _messagingService = messagingService;
    }

    protected override Task<AuthorizedRequest<Message>> ExecuteAsync() {
        _messagingService.MessageSent(_message);
        return new Task<AuthorizedRequest<Message>>(() => _message);
    }
}

public class AuthorizedRequest<T> {
    public readonly long UserId;
    public readonly T Request;
    public readonly UserConnection UserConnection;

    public AuthorizedRequest (long userId, T request, UserConnection userConnection) {
        UserId = userId;
        Request = request;
        UserConnection = userConnection;
    }
}
public class DeleteMessageEvent : EventBase<AuthorizedRequest<DeleteMessageRequest>> {
    public DeleteMessageEvent(Action sideEffect) : base(sideEffect) { }
    protected override Task<AuthorizedRequest<DeleteMessageRequest>> ExecuteAsync() {
        throw new NotImplementedException();
    }
}


public class RequestUpdateEvent : EventBase<AuthorizedRequest<RequestUpdate>> {
    private readonly AuthorizedRequest<RequestUpdate> _authorizedRequest;
    private readonly IMessagingService _messagingService;

    public RequestUpdateEvent(AuthorizedRequest<RequestUpdate> authorizedRequest, IMessagingService messagingService,
        Action sideEffect) : base(sideEffect) {
        _authorizedRequest = authorizedRequest;
        _messagingService = messagingService;
    }
    protected override async Task<AuthorizedRequest<RequestUpdate>> ExecuteAsync() {
        await _messagingService.UpdateRequested(_authorizedRequest);
        return _authorizedRequest;
    }
}

public class RequestDialogEvent : EventBase<AuthorizedRequest<RequestDialog>> {
    private readonly AuthorizedRequest<RequestDialog> _authorizedRequest;
    private readonly IMessagingService _messagingService;

    public RequestDialogEvent(AuthorizedRequest<RequestDialog> request, IMessagingService messagingService,
        Action sideEffect) : base(sideEffect) {
        _authorizedRequest = request;
        _messagingService = messagingService;
    }
    protected override async Task<AuthorizedRequest<RequestDialog>> ExecuteAsync() {
        await _messagingService.DialogRequested(_authorizedRequest);
        return _authorizedRequest;
    }
}

public class MessagePersistanceEvent : EventBase<MessageDTO> {
    private readonly MessageDTO _messageDto;
    private readonly IPersistenceService _service;
    public MessagePersistanceEvent(MessageDTO messageDto, IPersistenceService service,Action sideEffect) : base(sideEffect) {
        _messageDto = messageDto;
        _service = service;
    }
    protected override async Task<MessageDTO> ExecuteAsync() {
        await _service.SaveAsync(_messageDto.ConvertToDbModel());
        return _messageDto;
    }
}


public class CreateGroupEvent : EventBase<AuthorizedRequest<CreateGroupRequest>> {
    private readonly AuthorizedRequest<CreateGroupRequest> _authorizedRequest;
    private readonly IGroupService _groupService;
    public CreateGroupEvent(AuthorizedRequest<CreateGroupRequest> authorizedRequest, IGroupService groupService, Action sideEffect) : base(sideEffect) {
        _authorizedRequest = authorizedRequest;
        _groupService = groupService;
    }
    protected override async Task<AuthorizedRequest<CreateGroupRequest>> ExecuteAsync() {
        await _groupService.CreateGroup(_authorizedRequest);
        return _authorizedRequest;
    }
}

public class DeleteGroupEvent : EventBase<AuthorizedRequest<DeleteGroupRequest>> {
    private readonly AuthorizedRequest<DeleteGroupRequest> _authorizedRequest;
    private readonly IGroupService _groupService;
    public DeleteGroupEvent(AuthorizedRequest<DeleteGroupRequest> authorizedRequest, IGroupService groupService, Action sideEffect) : base(sideEffect) {
        _authorizedRequest = authorizedRequest;
        _groupService = groupService;
    }
    protected override async Task<AuthorizedRequest<DeleteGroupRequest>> ExecuteAsync() {
        await _groupService.DeleteGroup(_authorizedRequest);
        return _authorizedRequest;
    }
}

public class AddMemberEvent : EventBase<AuthorizedRequest<AddMemberRequest>> {
    private readonly AuthorizedRequest<AddMemberRequest> _authorizedRequest;
    private readonly IGroupService _groupService;
    public AddMemberEvent(AuthorizedRequest<AddMemberRequest> authorizedRequest, IGroupService groupService, Action sideEffect) : base(sideEffect) {
        _authorizedRequest = authorizedRequest;
        _groupService = groupService;
    }
    protected override async Task<AuthorizedRequest<AddMemberRequest>> ExecuteAsync() {
        await _groupService.AddMember(_authorizedRequest);
        return _authorizedRequest;
    }
}

public class RemoveMemberEvent : EventBase<AuthorizedRequest<RemoveMemberRequest>> {
    private readonly AuthorizedRequest<RemoveMemberRequest> _authorizedRequest;
    private readonly IGroupService _groupService;
    public RemoveMemberEvent(AuthorizedRequest<RemoveMemberRequest> authorizedRequest, IGroupService groupService, Action sideEffect) : base(sideEffect) {
        _authorizedRequest = authorizedRequest;
        _groupService = groupService;
    }
    protected override async Task<AuthorizedRequest<RemoveMemberRequest>> ExecuteAsync() {
        await _groupService.RemoveMember(_authorizedRequest);
        return _authorizedRequest;
    }
}


public class GroupCreatedEvent : EventBase<Group> {
    private readonly Group _group;
    private readonly IConnectionManager _connectionManager;
    public GroupCreatedEvent(Group group, IConnectionManager connectionManager, Action sideEffect) : base(sideEffect) {
        _group = group;
        _connectionManager = connectionManager;
    }
    protected override async Task<Group> ExecuteAsync() {
        await _connectionManager.HandleGroupCreated(_group);
        return _group;
    }
}
// SearchEvent
public class SearchEvent : EventBase<AuthorizedRequest<SearchRequest>> {
    
    private readonly AuthorizedRequest<SearchRequest> _searchRequest;
    private readonly ISearchService _service;

    public SearchEvent(AuthorizedRequest<SearchRequest> searchRequest, ISearchService service, Action sideEffect) :
        base(sideEffect) {
        _searchRequest = searchRequest;
        _service = service;
    }
    

    protected override async Task<AuthorizedRequest<SearchRequest>> ExecuteAsync() {
        await _service.Search(_searchRequest);
        return _searchRequest;
    }    
}

public class FetchUsersEvent : EventBase<AuthorizedRequest<FetchUserInfoRequest>> {
    private readonly AuthorizedRequest<FetchUserInfoRequest> _authorizedRequest;
    private readonly ISearchService _userService;

    public FetchUsersEvent(AuthorizedRequest<FetchUserInfoRequest> authorizedRequest, ISearchService searchService, Action sideEffect) : base(sideEffect) {
        _authorizedRequest = authorizedRequest;
        _userService = searchService;
    }

    protected override async Task<AuthorizedRequest<FetchUserInfoRequest>> ExecuteAsync() {
        await _userService.FetchUsers(_authorizedRequest);
        return _authorizedRequest;
    }
}


public class SetUserStatusEvent : EventBase<AuthorizedRequest<SetPersonStatus>> {
    private readonly AuthorizedRequest<SetPersonStatus> _authorizedRequest;
    private readonly ISearchService _userService;

    public SetUserStatusEvent(AuthorizedRequest<SetPersonStatus> authorizedRequest, ISearchService searchService, Action sideEffect) : base(sideEffect) {
        _authorizedRequest = authorizedRequest;
        _userService = searchService;
    }

    protected override async Task<AuthorizedRequest<SetPersonStatus>> ExecuteAsync() {
        await _userService.SetUserStatus(_authorizedRequest);
        return _authorizedRequest;
    }
}