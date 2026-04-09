using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(80)]
        public string Course { get; set; } = string.Empty;

        [Required]
        public DateTime Deadline { get; set; }

        [Range(1, 12)]
        public int EstimatedHours { get; set; }

        [Range(1, 5)]
        public int Priority { get; set; }

        [Range(1, 5)]
        public int Difficulty { get; set; }
    }
}
