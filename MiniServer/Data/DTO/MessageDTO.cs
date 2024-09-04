using Google.Protobuf.WellKnownTypes;
using MiniProtoImpl;
using MiniServer.Core.Events;

namespace MiniServer.Data.DTO; 

public class MessageDTO {
    public MessageDTO(AuthorizedRequest<Message> request) {
        this.Content = request.Request.Content;
        this.UserId = request.UserId;
        this.ReceiverId = request.Request.ReceiverId;
        if (request.Request.Timestamp == null) {
            this.Timestamp = DateTime.UtcNow;
        } else this.Timestamp = request.Request.Timestamp.ToDateTime();
        this.ResponseToId = request.Request.ResponsesTo;
        this.MessageType = request.Request.MsgType;
        this.IsEdited = request.Request.IsEdited;
        this.TargetId = request.Request.TargetId;
    }
    
    public MessageDTO(Data.Model.Message databaseMessage) {
        this.Content = databaseMessage.Content;
        this.UserId = databaseMessage.UserId;
        this.ReceiverId = databaseMessage.ReceiverId;
        this.Timestamp = databaseMessage.Timestamp;
        this.ResponseToId = databaseMessage.ResponseToId;
        this.MessageType = databaseMessage.MessageType;
        this.IsEdited = databaseMessage.isEdited;
        this.TargetId = databaseMessage.TargetId;
    }

    public long MessageId { get; set; }

    public string Content { get; set; }

    // Foreign key for the user who sent the message
    public long UserId { get; set; }
    
    public long ReceiverId { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime Timestamp { get; set; }
    
    public long ResponseToId { get; set; }
    
    public bool IsEdited { get; set; } = false;
    
    public int MessageType { get; set; } = 0;

    public long TargetId { get; set; }
    
    public Message ConvertToGrpcMessage() {
        return new Message {
            Id = MessageId,
            Content = Content,
            AuthorId = UserId,
            ReceiverId = ReceiverId,
            Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(Timestamp)
            // ,IsDeleted = isDeleted
            ,ResponsesTo = ResponseToId,
            MsgType = MessageType
            ,IsEdited = IsEdited
            ,TargetId = TargetId
        };
    }

    public Data.Model.Message ConvertToDbModel() {
        return new Data.Model.Message {
            MessageId = MessageId,
            Content = Content,
            UserId = UserId,
            ReceiverId = ReceiverId,
            isDeleted = IsDeleted,
            // TODO Attachments
            Timestamp = Timestamp,
            ResponseToId = ResponseToId,
            isEdited = IsEdited,
            MessageType = MessageType
            
        };        
        
    }
}