using MiniServer.Services;

namespace MiniServer.Events;

public class RegisterEvent : EventBase<RegisterResponse>
{
    private readonly RegisterRequest _request;
    private readonly IChatLogicService _chatLogicService;

    public RegisterEvent(RegisterRequest request, IChatLogicService chatLogicService, Action logic)
        : base(logic)
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