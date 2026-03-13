using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class CreateAvailabilityDto
    {
        [Required]
        [RegularExpression("Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday")]
        public string DayOfWeek { get; set; } = string.Empty;

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Range(1, 12)]
        public int MaxStudyHours { get; set; } = 4;
    }
}
