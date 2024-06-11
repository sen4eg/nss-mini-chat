using System.ComponentModel.DataAnnotations;

namespace MiniServer.Data.Model;

public class Attachment
{
    [Key]
    public int AttachmentId { get; set; }

    [Required]
    public byte[] Content { get; set; }

    [Required]
    public string Filename { get; set; }

    // Foreign key for the message this attachment belongs to
    public int MessageId { get; set; }

    // Navigation property for the message this attachment belongs to
    public Message Message { get; set; }
}