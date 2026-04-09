using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.Models
{
    public class SystemSetting
    {
        public int Id { get; set; }

        [Range(1, 12)]
        public int DefaultMaxStudyHoursPerDay { get; set; } = 4;

        [Range(0, 23)]
        public int DefaultStartHour { get; set; } = 16;

        [Range(1, 23)]
        public int DefaultEndHour { get; set; } = 22;

        [Range(0, 120)]
        public int DefaultBreakMinutes { get; set; } = 15;
    }
}
