namespace SmartSched.Api.DTOs
{
    public class ScheduleSuggestionDto
    {
        public int ScheduleSuggestionId { get; set; }
        public int TaskItemId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int AllocatedHours { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
