using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSched.Api.Models
{
    public class CourseClass
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Semester { get; set; } = string.Empty;

        [Required]
        public string ProfessorId { get; set; } = string.Empty;

        [ForeignKey(nameof(ProfessorId))]
        public ApplicationUser? Professor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}