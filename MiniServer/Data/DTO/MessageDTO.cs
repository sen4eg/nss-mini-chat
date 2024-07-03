using Google.Protobuf.WellKnownTypes;
using MiniProtoImpl;
using MiniServer.Core.Events;

namespace MiniServer.Data.DTO; 

public class MessageDTO {
    public MessageDTO(AuthorizedRequest<Message> request) {
        this.Content = request.Request.Message_;
        this.UserId = request.UserId;
        this.ReceiverId = request.Request.ReceiverId;
        this.Timestamp = DateTime.UtcNow;
    }
    
    public MessageDTO(Data.Model.Message databaseMessage) {
        this.Content = databaseMessage.Content;
        this.UserId = databaseMessage.UserId;
        this.ReceiverId = databaseMessage.ReceiverId;
        this.Timestamp = DateTime.UtcNow;
    }

    public long MessageId { get; set; }

    public string Content { get; set; }

    // Foreign key for the user who sent the message
    public long UserId { get; set; }
    
    public long ReceiverId { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime Timestamp { get; set; }
    public Message ConvertToGrpcMessage() {
        return new Message {
            Id = MessageId,
            Message_ = Content,
            AuthorId = UserId,
            ReceiverId = ReceiverId,
            Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(Timestamp)
            // IsDeleted = isDeleted
        };
    }

    public Data.Model.Message ConvertToDbModel() {
        return new Data.Model.Message {
            MessageId = MessageId,
            Content = Content,
            UserId = UserId,
            ReceiverId = ReceiverId,
            // IsDeleted = IsDeleted,
            // TODO Attachments
            Timestamp = Timestamp
        };        
        
    }
}