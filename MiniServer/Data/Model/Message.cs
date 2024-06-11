﻿using System.ComponentModel.DataAnnotations;

namespace MiniServer.Data.Model;

public class Message
{
    [Key]
    public int MessageId { get; set; }

    [Required]
    public string Content { get; set; }

    // Foreign key for the user who sent the message
    public int UserId { get; set; }

    // Navigation property for the user who sent the message
    public User Sender { get; set; }

    // Navigation property for attachments related to this message
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}