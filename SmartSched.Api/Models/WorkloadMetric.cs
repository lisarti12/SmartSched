using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSched.Api.Models
{
    public class WorkloadMetric
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public ApplicationUser? Student { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public int TotalScheduledHours { get; set; }

        public int WorkloadScore { get; set; }

        [Required]
        [MaxLength(20)]
        public string WorkloadLevel { get; set; } = "Low"; // Low, Medium, High

        [MaxLength(250)]
        public string WarningMessage { get; set; } = string.Empty;
    }
}
