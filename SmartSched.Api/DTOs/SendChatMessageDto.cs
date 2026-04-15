using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class SendChatMessageDto
    {
        [Required]
        public string ReceiverId { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string MessageText { get; set; } = string.Empty;
    }
}