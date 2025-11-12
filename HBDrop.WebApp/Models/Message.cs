using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBDrop.WebApp.Models;

/// <summary>
/// Message status enum
/// </summary>
public enum MessageStatus
{
    Pending,
    Sent,
    Failed,
    Delivered,
    Read
}

/// <summary>
/// Represents a WhatsApp message sent via the system
/// </summary>
public class Message
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the user who sent this message
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the sender
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Foreign key to the contact who received this message
    /// </summary>
    [Required]
    public int ContactId { get; set; }

    /// <summary>
    /// Navigation property to the recipient
    /// </summary>
    [ForeignKey(nameof(ContactId))]
    public Contact Contact { get; set; } = null!;

    /// <summary>
    /// Message content
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the message
    /// </summary>
    [Required]
    public MessageStatus Status { get; set; } = MessageStatus.Pending;

    /// <summary>
    /// Error message if sending failed
    /// </summary>
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether this was an automated birthday message
    /// </summary>
    public bool IsBirthdayMessage { get; set; } = false;

    /// <summary>
    /// When the message was created/queued
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the message was actually sent
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// When the message was delivered (if available from WhatsApp)
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// When the message was read (if available from WhatsApp)
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Number of retry attempts if sending failed
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Maximum retry attempts allowed
    /// </summary>
    public const int MaxRetryAttempts = 3;
}
