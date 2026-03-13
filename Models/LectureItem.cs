using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSched.Api.Models
{
    public class LectureItem
    {
        public int Id { get; set; }

        [Required]
        public int CourseClassId { get; set; }

        [ForeignKey(nameof(CourseClassId))]
        public CourseClass? CourseClass { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(300)]
        public string FilePath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}