using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(80)]
        public string Course { get; set; } = string.Empty;

        [Required]
        public DateTime Deadline { get; set; }

        [Range(1, 12)]
        public int EstimatedHours { get; set; }

        [Range(1, 5)]
        public int Priority { get; set; }

        [Range(1, 5)]
        public int Difficulty { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Scheduled, Completed

        public bool IsProfessorAssigned { get; set; } = false;

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public ApplicationUser? Student { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
