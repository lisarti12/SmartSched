using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSched.Api.Models
{
    public class CourseEnrollment
    {
        public int Id { get; set; }

        [Required]
        public int CourseClassId { get; set; }

        [ForeignKey(nameof(CourseClassId))]
        public CourseClass? CourseClass { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public ApplicationUser? Student { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}