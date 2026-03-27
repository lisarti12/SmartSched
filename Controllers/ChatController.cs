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
    [Authorize(Roles = "Student,Professor")]
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
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains("Student"))
            {
                var contacts = await _context.CourseEnrollments
                    .Where(e => e.StudentId == currentUserId)
                    .Join(
                        _context.CourseClasses,
                        e => e.CourseClassId,
                        c => c.Id,
                        (e, c) => c
                    )
                    .Join(
                        _context.Users,
                        c => c.ProfessorId,
                        u => u.Id,
                        (c, u) => new
                        {
                            id = u.Id,
                            fullName = u.FirstName + " " + u.LastName,
                            email = u.Email
                        }
                    )
                    .Distinct()
                    .ToListAsync();

                var unreadCounts = await _context.ChatMessages
                    .Where(m => m.ReceiverId == currentUserId && !m.IsRead)
                    .GroupBy(m => m.SenderId)
                    .Select(g => new
                    {
                        userId = g.Key,
                        count = g.Count()
                    })
                    .ToListAsync();

                var result = contacts.Select(c => new
                {
                    id = c.id,
                    fullName = c.fullName,
                    email = c.email,
                    unreadCount = unreadCounts.FirstOrDefault(x => x.userId == c.id)?.count ?? 0
                });

                return Ok(result);
            }

            if (roles.Contains("Professor"))
            {
                var contacts = await _context.CourseClasses
                    .Where(c => c.ProfessorId == currentUserId)
                    .Join(
                        _context.CourseEnrollments,
                        c => c.Id,
                        e => e.CourseClassId,
                        (c, e) => e
                    )
                    .Join(
                        _context.Users,
                        e => e.StudentId,
                        u => u.Id,
                        (e, u) => new
                        {
                            id = u.Id,
                            fullName = u.FirstName + " " + u.LastName,
                            email = u.Email
                        }
                    )
                    .Distinct()
                    .ToListAsync();

                var unreadCounts = await _context.ChatMessages
                    .Where(m => m.ReceiverId == currentUserId && !m.IsRead)
                    .GroupBy(m => m.SenderId)
                    .Select(g => new
                    {
                        userId = g.Key,
                        count = g.Count()
                    })
                    .ToListAsync();

                var result = contacts.Select(c => new
                {
                    id = c.id,
                    fullName = c.fullName,
                    email = c.email,
                    unreadCount = unreadCounts.FirstOrDefault(x => x.userId == c.id)?.count ?? 0
                });

                return Ok(result);
            }

            return Forbid();
        }

        [HttpGet("unread-total")]
        public async Task<IActionResult> GetUnreadTotal()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var unreadTotal = await _context.ChatMessages
                .CountAsync(m => m.ReceiverId == currentUserId && !m.IsRead);

            return Ok(new { unreadTotal });
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<IActionResult> GetConversation(string otherUserId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            if (!await CanUsersChatAsync(currentUserId, otherUserId))
                return Forbid();

            var unreadMessages = await _context.ChatMessages
                .Where(m =>
                    m.SenderId == otherUserId &&
                    m.ReceiverId == currentUserId &&
                    !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }

            var messages = await _context.ChatMessages
                .Where(m =>
                    (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                    (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    id = m.Id,
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    messageText = m.MessageText,
                    sentAt = m.SentAt,
                    isRead = m.IsRead
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendChatMessageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.ReceiverId) || string.IsNullOrWhiteSpace(dto.MessageText))
                return BadRequest(new { message = "Receiver and message are required." });

            if (senderId == dto.ReceiverId)
                return BadRequest(new { message = "You cannot message yourself." });

            if (!await CanUsersChatAsync(senderId, dto.ReceiverId))
            {
                return BadRequest(new
                {
                    message = "Only student-professor chat is allowed, and only if they are connected through a course."
                });
            }

            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                MessageText = dto.MessageText.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = message.Id,
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                messageText = message.MessageText,
                sentAt = message.SentAt,
                isRead = message.IsRead
            });
        }

        private async Task<bool> CanUsersChatAsync(string firstUserId, string secondUserId)
        {
            var firstUser = await _userManager.FindByIdAsync(firstUserId);
            var secondUser = await _userManager.FindByIdAsync(secondUserId);

            if (firstUser == null || secondUser == null)
                return false;

            var firstRoles = await _userManager.GetRolesAsync(firstUser);
            var secondRoles = await _userManager.GetRolesAsync(secondUser);

            var firstIsStudent = firstRoles.Contains("Student");
            var firstIsProfessor = firstRoles.Contains("Professor");
            var secondIsStudent = secondRoles.Contains("Student");
            var secondIsProfessor = secondRoles.Contains("Professor");

            if (firstIsStudent && secondIsProfessor)
            {
                return await _context.CourseEnrollments
                    .Where(e => e.StudentId == firstUserId)
                    .Join(
                        _context.CourseClasses,
                        e => e.CourseClassId,
                        c => c.Id,
                        (e, c) => c
                    )
                    .AnyAsync(c => c.ProfessorId == secondUserId);
            }

            if (firstIsProfessor && secondIsStudent)
            {
                return await _context.CourseClasses
                    .Where(c => c.ProfessorId == firstUserId)
                    .Join(
                        _context.CourseEnrollments,
                        c => c.Id,
                        e => e.CourseClassId,
                        (c, e) => e
                    )
                    .AnyAsync(e => e.StudentId == secondUserId);
            }

            return false;
        }
    }
}