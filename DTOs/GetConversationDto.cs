using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class GetConversationDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}