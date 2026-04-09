namespace SmartSched.Api.Models
{
    public class SmartSchedRunLog
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public DateTime RunAt { get; set; } = DateTime.UtcNow;
    }
}   