using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.Models
{
    public class AvailabilityRule
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public ApplicationUser? Student { get; set; }

        [Required]
        [MaxLength(20)]
        public string DayOfWeek { get; set; } = string.Empty; // Monday, Tuesday...

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Range(1, 12)]
        public int MaxStudyHours { get; set; } = 4;

        public bool IsAvailable { get; set; } = true;
    }
}
