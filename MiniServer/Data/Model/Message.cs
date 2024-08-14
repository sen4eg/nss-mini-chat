using System.ComponentModel.DataAnnotations;
using MiniServer.Data.DTO;

namespace MiniServer.Data.Model;

public class Message
{
    [Key]
    public long MessageId { get; set; }

    [Required]
    public string Content { get; set; }

    // Foreign key for the user who sent the message
    public long UserId { get; set; }

    // Navigation property for the user who sent the message
    public User Sender { get; set; }

    // Navigation property for attachments related to this message
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    
    public long ReceiverId { get; set; }
    
    public long ResponseToId { get; set; }
    public bool isDeleted { get; set; } = false;
    
    public bool isEdited { get; set; } = false;
    
    public DateTime Timestamp { get; set; }

    public int MessageType { get; set; }
    
    // Due to the nature of edit,delete technical messages, we need to keep the original message
    public long TargetId { get; set; }

    public void EditFromDTO(MessageDTO message) {
        this.Content = message.Content;
        this.isEdited = true;
        this.Timestamp = DateTime.UtcNow;
        this.ResponseToId = message.ResponseToId;
    }
}