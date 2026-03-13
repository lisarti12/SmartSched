using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.Models
{
    public class ScheduleSuggestion
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public ApplicationUser? Student { get; set; }

        [Required]
        public int TaskItemId { get; set; }

        [ForeignKey(nameof(TaskItemId))]
        public TaskItem? TaskItem { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public int AllocatedHours { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Suggested"; // Suggested, Accepted, Skipped

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
