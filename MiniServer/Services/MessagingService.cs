﻿using MiniProtoImpl;
using MiniServer.Core;
using MiniServer.Core.Events;
using MiniServer.Data.DTO;
using MiniServer.Data.Repository;

namespace MiniServer.Services; 

public interface IMessagingService
{
    void MessageSent(AuthorizedRequest<Message> request);
    Task UpdateRequested(AuthorizedRequest<RequestUpdate> authorizedRequest);
    Task DialogRequested(AuthorizedRequest<RequestDialog> authorizedRequest);
    Task HandleMsgSave(MessageDTO messageDto);
}

public class MessagingService : IMessagingService {
    private long _lastUsedMessageId;
    private IConnectionManager _connectionManager;
    private EventDispatcher _eventDispatcher;
    private long GetNextMessageId() {
        return _lastUsedMessageId = Interlocked.Increment(ref _lastUsedMessageId);
    }
    
    private readonly ICommEventFactory _commEventFactory;
    private readonly IServiceScopeFactory _scopedServiceFactory;


    public MessagingService(ICommEventFactory commEventFactory, EventDispatcher eventDispatcher, 
            IConnectionManager connectionManager, IServiceScopeFactory scopedServiceFactory) {
        _commEventFactory = commEventFactory;
        _eventDispatcher = eventDispatcher;
        _connectionManager = connectionManager;
        using (var scope = scopedServiceFactory.CreateScope()) {
            IMessageRepository repository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
            _lastUsedMessageId = repository.GetLastMessageId().GetAwaiter().GetResult();
        }
        _scopedServiceFactory = scopedServiceFactory;
    }


    public void MessageSent(AuthorizedRequest<Message> request) {
        if (request.Request.ReceiverId >= 0) {
            HandlePrivateMessage(request);
        }
        HandleGroupMessage(request);
    }

    public Task UpdateRequested(AuthorizedRequest<RequestUpdate> authorizedRequest) {
//        var messages = _messageRepository.GetDialogsForUser(authorizedRequest.UserId, authorizedRequest.Request.LastMessageId);
        var cons = _connectionManager.GetOpenedConnection(authorizedRequest.UserId);
        if (cons == null) {
            return Task.CompletedTask;
        }

        using (var scope = _scopedServiceFactory.CreateScope()){
            var contactService = scope.ServiceProvider.GetRequiredService<IContactService>();
            List<Dialog> dialogs = contactService.GetDialogsForUser(authorizedRequest.UserId);
            ContactsMessage contactsMessage = new ContactsMessage {
                Dialogs = { dialogs }
            };
            var response = new CommunicationResponse {
                ContactsMessage = contactsMessage
            };
            foreach (var userConnection in cons) {
                userConnection.ResponseStream.WriteAsync(response);
            }
            return Task.CompletedTask;
        }

    }

    public Task DialogRequested(AuthorizedRequest<RequestDialog> authorizedRequest) {
        using (var scope = _scopedServiceFactory.CreateScope()) {

            var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
            var dialog = messageRepository.GetMessagesForUser(authorizedRequest.UserId, authorizedRequest.Request);

            var cons = _connectionManager.GetOpenedConnection(authorizedRequest.UserId);
            if (cons == null) {
                return Task.CompletedTask;
            }

            DialogBody dialogBody = new DialogBody {
                ContactId = dialog.DialogId,
                LastMessageId = dialog.lastMessageId,
                Messages = { dialog.messages.Select(m => m.ConvertToGrpcMessage()) }
            };
            var response = new CommunicationResponse {
                DialogUpdate = dialogBody
            };
            foreach (var userConnection in cons) {
                userConnection.ResponseStream.WriteAsync(response);
            }

            return Task.CompletedTask;
        }
    }

    public Task HandleMsgSave(MessageDTO messageDto) {
        using (var scope = _scopedServiceFactory.CreateScope()) {
            var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
            switch (messageDto.MessageType) {
                case 1: {
                    messageRepository.UpdateMessageAsync(messageDto);
                    break;
                }
                case 2:
                    messageRepository.DeleteMessageAsync(messageDto.TargetId, messageDto.UserId);
                    break;
            }

            messageRepository.CreateMessageAsync(messageDto);
            return Task.CompletedTask;
        }
    }

    private void HandlePrivateMessage(AuthorizedRequest<Message> request) {
        using (var scope = _scopedServiceFactory.CreateScope()) {
            var contactService = scope.ServiceProvider.GetRequiredService<IContactService>();
            var msg = new MessageDTO(request);
            msg.MessageId = GetNextMessageId();


            var messageEvent = _commEventFactory.Create<MessagePersistanceEvent, MessageDTO>(msg, () => { });
            var cons = _connectionManager.GetOpenedConnection(msg.ReceiverId);
            var senderCons = _connectionManager.GetOpenedConnection(request.UserId);
            FireMessage(senderCons, msg); // tell the sender the message was sent
            FireMessage(cons, msg); // fire right away persist slowly
            _eventDispatcher.EnqueueEvent(
                async () => await messageEvent.Execute(new TaskCompletionSource<MessageDTO>()));
            if (msg.MessageType == 0) contactService.UpdateDialog(msg.UserId, msg.ReceiverId);
        }
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

    private async Task HandleGroupMessage(AuthorizedRequest<Message> message) {
        using (var scope = _scopedServiceFactory.CreateScope()) {
            var groupRepository = scope.ServiceProvider.GetRequiredService<IGroupRepository>();

            var group = await groupRepository.GetGroup(message.Request.ReceiverId);
            if (group == null) {
                return;
            }

            var msg = new MessageDTO(message);
            msg.MessageId = GetNextMessageId();
            var messageEvent = _commEventFactory.Create<MessagePersistanceEvent, MessageDTO>(msg, () => { });
            var members = group.GroupRoles.Select(g => g.UserId).ToList();
            members.Add(group.CreatorUserId);
            foreach (var groupUser in members) {
                var cons = _connectionManager.GetOpenedConnection(groupUser);
                FireMessage(cons, msg);
            }

            _eventDispatcher.Fire(messageEvent, new TaskCompletionSource<MessageDTO>());
        }
    }
}