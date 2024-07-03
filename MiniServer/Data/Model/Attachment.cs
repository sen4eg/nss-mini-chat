using System.ComponentModel.DataAnnotations;

namespace MiniServer.Data.Model;

public class Attachment
{
    [Key]
    public long AttachmentId { get; set; }

    [Required]
    public byte[] Content { get; set; }

    [Required]
    public string Filename { get; set; }

    // Foreign key for the message this attachment belongs to
    public long MessageId { get; set; }

    // Navigation property for the message this attachment belongs to
    public MiniServer.Data.Model.Message Message { get; set; }
}