using MiniProtoImpl;
using MiniServer.Data.DTO;
using MiniServer.Services;

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
    public long UserId;
    public T Request;

    public AuthorizedRequest (long userId, T request) {
        UserId = userId;
        Request = request;
    }
}
public class DeleteMessageEvent : EventBase<AuthorizedRequest<DeleteMessageRequest>> {
    public DeleteMessageEvent(Action sideEffect) : base(sideEffect) { }
    protected override Task<AuthorizedRequest<DeleteMessageRequest>> ExecuteAsync() {
        throw new NotImplementedException();
    }
}


public class RequestUpdateEvent : EventBase<AuthorizedRequest<RequestUpdate>> {
    public RequestUpdateEvent(Action sideEffect) : base(sideEffect) { }
    protected override Task<AuthorizedRequest<RequestUpdate>> ExecuteAsync() {
        throw new NotImplementedException();
    }
}

public class RequestDialogEvent : EventBase<AuthorizedRequest<RequestDialog>> {
    public RequestDialogEvent(Action sideEffect) : base(sideEffect) { }
    protected override Task<AuthorizedRequest<RequestDialog>> ExecuteAsync() {
        throw new NotImplementedException();
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