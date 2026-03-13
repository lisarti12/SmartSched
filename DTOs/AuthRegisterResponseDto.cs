namespace SmartSched.Api.DTOs
{
    public class AuthRegisterResponseDto
    {
        public bool RequiresApproval { get; set; }
        public string Message { get; set; } = string.Empty;

        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }
}