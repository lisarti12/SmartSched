using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class CreateCourseContentItemDto
    {
        [Required]
        [RegularExpression("Homework|Quiz|Project")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(150, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }
    }
}