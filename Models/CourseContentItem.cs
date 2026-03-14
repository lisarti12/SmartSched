using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSched.Api.Models
{
    public class CourseContentItem
    {
        public int Id { get; set; }

        [Required]
        public int CourseClassId { get; set; }

        [ForeignKey(nameof(CourseClassId))]
        public CourseClass? CourseClass { get; set; }

        [Required]
        [MaxLength(30)]
        public string Type { get; set; } = string.Empty; // Homework, Quiz, Project

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        [MaxLength(300)]
        public string FilePath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}