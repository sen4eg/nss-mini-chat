using MiniProtoImpl;
using MiniServer.Core;
using MiniServer.Core.Events;
using MiniServer.Data.DTO;
using MiniServer.Data.Repository;

namespace MiniServer.Services; 

public interface IMessagingService
{
    void MessageSent(AuthorizedRequest<Message> request);
}

public class MessagingService : IMessagingService {
    private long _lastUsedMessageId;
    private IConnectionManager _connectionManager;
    private EventDispatcher _eventDispatcher;
    private long GetNextMessageId() {
        return _lastUsedMessageId = Interlocked.Increment(ref _lastUsedMessageId);
    }
    
    private readonly ICommEventFactory _commEventFactory;
    
    public MessagingService(ICommEventFactory commEventFactory, EventDispatcher eventDispatcher, IConnectionManager connectionManager, IMessageRepository repository) {
        _commEventFactory = commEventFactory;
        _eventDispatcher = eventDispatcher;
        _connectionManager = connectionManager;
        _lastUsedMessageId = repository.GetLastMessageId().GetAwaiter().GetResult();// TODO check if this is correct
    }


    public void MessageSent(AuthorizedRequest<Message> request) {
        if (request.Request.ReceiverId >= 0) {
            HandlePrivateMessage(request);
        }
        HandleGroupMessage(request);
    }

    private void HandlePrivateMessage(AuthorizedRequest<Message> request) {
        var msg = new MessageDTO(request);
        msg.MessageId = GetNextMessageId();
        
        var messageEvent = _commEventFactory.Create<MessagePersistanceEvent, MessageDTO>(msg, () => { });
        var cons = _connectionManager.GetOpenedConnection(msg.ReceiverId);
        FireMessage(cons, msg); // fire right away persist slowly
        // TODO Consider caching here
        _eventDispatcher.EnqueueEvent(() => messageEvent.Execute(new TaskCompletionSource<MessageDTO>()));
    }

    private static void FireMessage(IEnumerable<UserConnection>? userConnections, MessageDTO msg) {
        if (userConnections == null) {
            return;
        }
        var payload = msg.ConvertToGrpcMessage();
        foreach (var userConnection in userConnections) {
            Console.WriteLine(userConnection.GetHashCode()+ " " + userConnection.Context.GetHashCode());
            // TODO consider error proofing this
            var response = new CommunicationResponse {
                Message = payload
            };
            userConnection.ResponseStream.WriteAsync(
                response);
        }
    }

    private void HandleGroupMessage(AuthorizedRequest<Message> message) {
        throw new NotImplementedException();
    }
}