using System.ComponentModel.DataAnnotations;

namespace SmartSched.Api.DTOs
{
    public class UpdateSystemSettingDto
    {

        [Range(0, 23)]
        public int DefaultStartHour { get; set; }

        [Range(1, 23)]
        public int DefaultEndHour { get; set; }

        [Range(0, 120)]
        public int DefaultBreakMinutes { get; set; }
    }
}
