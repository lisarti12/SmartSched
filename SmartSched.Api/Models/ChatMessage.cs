using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSched.Api.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [ForeignKey(nameof(SenderId))]
        public ApplicationUser? Sender { get; set; }

        [Required]
        public string ReceiverId { get; set; } = string.Empty;

        [ForeignKey(nameof(ReceiverId))]
        public ApplicationUser? Receiver { get; set; }

        [Required]
        [MaxLength(2000)]
        public string MessageText { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}