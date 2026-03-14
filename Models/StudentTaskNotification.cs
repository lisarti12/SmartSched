using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSched.Api.Models
{
    public class StudentTaskNotification
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public ApplicationUser? Student { get; set; }

        [Required]
        public int CourseClassId { get; set; }

        [ForeignKey(nameof(CourseClassId))]
        public CourseClass? CourseClass { get; set; }

        [Required]
        public int CourseContentItemId { get; set; }

        [ForeignKey(nameof(CourseContentItemId))]
        public CourseContentItem? CourseContentItem { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}