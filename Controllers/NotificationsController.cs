using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSched.Api.Data;
using SmartSched.Api.DTOs;
using System.Security.Claims;

namespace SmartSched.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notifications = await _context.StudentNotifications
                .Include(n => n.Announcement)
                .Where(n => n.StudentId == studentId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.Id,
                    AnnouncementId = n.AnnouncementId,
                    Title = n.Announcement!.Title,
                    Message = n.Announcement.Message,
                    Type = n.Announcement.Type,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead
                })
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notification = await _context.StudentNotifications
                .FirstOrDefaultAsync(n => n.Id == id && n.StudentId == studentId);

            if (notification == null)
                return NotFound(new { message = "Notification not found." });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notification marked as read." });
        }
    }
}
