using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class CreateAnnouncementDto
    {
        [Required]
        [StringLength(120, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 3)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [RegularExpression("Homework|Lecture|Assignment|Quiz", ErrorMessage = "Type must be Homework, Lecture, Assignment, or Quiz.")]
        public string Type { get; set; } = string.Empty;
    }
}
