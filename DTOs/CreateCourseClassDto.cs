using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class CreateCourseClassDto
    {
        [Required]
        [RegularExpression(@"^[A-Z]{3}\d{4}[A-Z]\s.+$", ErrorMessage = "Title must look have a 4-digit number then a letter")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 15)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [RegularExpression("Fall 26|Spring 27", ErrorMessage = "Semester must be Fall 26 or Spring 27.")]
        public string Semester { get; set; } = string.Empty;
    }
}