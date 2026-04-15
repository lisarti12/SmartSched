using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSched.Api.Data;
using SmartSched.Api.DTOs;
using SmartSched.Api.Models;
using System.Security.Claims;

namespace SmartSched.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(currentUser);
            var isStudent = roles.Contains("Student");
            var isProfessor = roles.Contains("Professor");

            IQueryable<ApplicationUser> contactsQuery;

            if (isStudent)
            {
                var professorIds = await _context.CourseEnrollments
                    .Where(e => e.StudentId == currentUserId)
                    .Join(
                        _context.CourseClasses,
                        e => e.CourseClassId,
                        c => c.Id,
                        (e, c) => c.ProfessorId)
                    .Distinct()
                    .ToListAsync();

                contactsQuery = _context.Users.Where(u => professorIds.Contains(u.Id));
            }
            else if (isProfessor)
            {
                var studentIds = await _context.CourseClasses
                    .Where(c => c.ProfessorId == currentUserId)
                    .Join(
                        _context.CourseEnrollments,
                        c => c.Id,
                        e => e.CourseClassId,
                        (c, e) => e.StudentId)
                    .Distinct()
                    .ToListAsync();

                contactsQuery = _context.Users.Where(u => studentIds.Contains(u.Id));
            }
            else
            {
                return Ok(new List<object>());
            }

            var contacts = await contactsQuery
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FirstName + " " + u.LastName,
                    email = u.Email
                })
                .ToListAsync();

            var unreadBySender = await _context.ChatMessages
                .Where(m => m.ReceiverId == currentUserId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                .Select(g => new
                {
                    senderId = g.Key,
                    unreadCount = g.Count()
                })
                .ToDictionaryAsync(x => x.senderId, x => x.unreadCount);

            var result = contacts
                .Select(c => new
                {
                    c.id,
                    c.fullName,
                    c.email,
                    unreadCount = unreadBySender.ContainsKey(c.id) ? unreadBySender[c.id] : 0
                })
                .OrderByDescending(c => c.unreadCount)
                .ThenBy(c => c.fullName)
                .ToList();

            return Ok(result);
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<IActionResult> GetConversation(string otherUserId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Unauthorized();

            var messages = await _context.ChatMessages
                .Where(m =>
                    (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                    (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.ReceiverId,
                    m.MessageText,
                    m.SentAt,
                    m.IsRead
                })
                .ToListAsync();

            var unreadIncoming = await _context.ChatMessages
                .Where(m => m.SenderId == otherUserId && m.ReceiverId == currentUserId && !m.IsRead)
                .ToListAsync();

            if (unreadIncoming.Count > 0)
            {
                foreach (var msg in unreadIncoming)
                {
                    msg.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }

            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendChatMessageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Unauthorized();

            if (dto.ReceiverId == currentUserId)
                return BadRequest(new { message = "You cannot message yourself." });

            var receiver = await _userManager.FindByIdAsync(dto.ReceiverId);
            if (receiver == null)
                return NotFound(new { message = "Receiver not found." });

            var message = new ChatMessage
            {
                SenderId = currentUserId,
                ReceiverId = dto.ReceiverId,
                MessageText = dto.MessageText.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Message sent.",
                item = new
                {
                    message.Id,
                    message.SenderId,
                    message.ReceiverId,
                    message.MessageText,
                    message.SentAt,
                    message.IsRead
                }
            });
        }

        [HttpGet("unread-total")]
        public async Task<IActionResult> GetUnreadTotal()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Unauthorized();

            var unreadTotal = await _context.ChatMessages
                .CountAsync(m => m.ReceiverId == currentUserId && !m.IsRead);

            return Ok(new { unreadTotal });
        }
    }
}