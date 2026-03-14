using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class AddStudentsToCourseDto
    {
        [Required]
        [MinLength(1)]
        public List<string> StudentIds { get; set; } = new();
    }
}