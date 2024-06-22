using MiniServer.Services;

namespace MiniServer.Core.Events;
public class RegisterEvent : EventBase<RegisterResponse>
{
    private readonly RegisterRequest _request;
    private readonly IChatLogicService _chatLogicService;

    public RegisterEvent(RegisterRequest request, IChatLogicService chatLogicService, Action sideEffect)
        : base(sideEffect)
    {
        _request = request;
        _chatLogicService = chatLogicService;
    }

    protected override async Task<RegisterResponse> ExecuteAsync()
    {
        // Process the registration logic
        return await _chatLogicService.RegisterAsync(_request);
    }
}

public class ConnectEvent : EventBase<ConnectResponse> {
    private readonly ConnectRequest _request;
        // any way it can be injected?
    private readonly IChatLogicService _chatLogicService;

    public ConnectEvent(ConnectRequest request, IChatLogicService chatLogicService, Action action) : base(action) {
        this._request = request;
        this._chatLogicService = chatLogicService;
    }

    protected override async Task<ConnectResponse> ExecuteAsync() {
        return await _chatLogicService.ConnectAsync(_request);
    }
}

public class RefreshTokenEvent : EventBase<ConnectResponse> {
    private readonly RefreshTokenRequest _request;
    private readonly IAuthenticationService _authenticationService;

    public RefreshTokenEvent(RefreshTokenRequest request, IAuthenticationService authenticationService, Action action) : base(action) {
        this._request = request;
        this._authenticationService = authenticationService;
    }

    protected override async Task<ConnectResponse> ExecuteAsync() {
        return await _authenticationService.RefreshTokenAsync(_request);
    }
}