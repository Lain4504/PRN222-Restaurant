using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Services.IService;
using System.Text.Json;

namespace PRN222_Restaurant.Pages.Admin.Chat
{
    public class IndexModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IChatService chatService, IUserService userService, ILogger<IndexModel> logger)
        {
            _chatService = chatService;
            _userService = userService;
            _logger = logger;
        }

        public int UserId { get; set; }
        public string UserRole { get; set; } = "Staff";

        public async Task<IActionResult> OnGetAsync()
        {
            UserId = HttpContext.Session.GetInt32("UserId") ?? 1;
            
            // Get user role
            var user = await _userService.GetUserByIdAsync(UserId);
            if (user == null || (user.Role != "Staff" && user.Role != "Admin"))
            {
                return Forbid("Access denied. Staff or Admin role required.");
            }

            UserRole = user.Role;
            return Page();
        }

        public async Task<IActionResult> OnGetGetChatRoomsAsync()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 1;
                var user = await _userService.GetUserByIdAsync(userId);
                
                if (user == null || (user.Role != "Staff" && user.Role != "Admin"))
                {
                    return Forbid();
                }

                IEnumerable<PRN222_Restaurant.Models.ChatRoom> chatRooms;
                
                if (user.Role == "Admin")
                {
                    // Admins can see all chat rooms
                    chatRooms = await _chatService.GetActiveChatRoomsAsync();
                }
                else
                {
                    // Staff can see their assigned rooms and unassigned rooms
                    var allRooms = await _chatService.GetActiveChatRoomsAsync();
                    chatRooms = allRooms.Where(cr => cr.StaffId == userId || cr.StaffId == null);
                }

                var chatRoomData = chatRooms.Select(cr => new
                {
                    id = cr.Id,
                    name = cr.Name,
                    customerId = cr.CustomerId,
                    staffId = cr.StaffId,
                    status = cr.Status,
                    createdAt = cr.CreatedAt,
                    lastMessageAt = cr.LastMessageAt,
                    customer = cr.Customer != null ? new
                    {
                        id = cr.Customer.Id,
                        fullName = cr.Customer.FullName,
                        email = cr.Customer.Email
                    } : null,
                    staff = cr.Staff != null ? new
                    {
                        id = cr.Staff.Id,
                        fullName = cr.Staff.FullName,
                        email = cr.Staff.Email
                    } : null,
                    lastMessage = cr.Messages?.OrderByDescending(m => m.SentAt).FirstOrDefault()?.Content,
                    unreadCount = cr.Messages?.Count(m => m.SenderId != userId && !m.IsRead) ?? 0
                }).OrderByDescending(cr => cr.lastMessageAt).ToList();

                return new JsonResult(chatRoomData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat rooms");
                return BadRequest(new { error = "Failed to load chat rooms" });
            }
        }

        public async Task<IActionResult> OnGetGetMessagesAsync(int chatRoomId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 1;
                var user = await _userService.GetUserByIdAsync(userId);
                
                if (user == null || (user.Role != "Staff" && user.Role != "Admin"))
                {
                    return Forbid();
                }

                // Verify access to chat room
                var chatRoom = await _chatService.GetChatRoomByIdAsync(chatRoomId);
                if (chatRoom == null)
                {
                    return NotFound();
                }

                // Check permissions
                if (user.Role != "Admin" && chatRoom.StaffId != userId && chatRoom.StaffId != null)
                {
                    return Forbid();
                }

                var messages = await _chatService.GetRecentMessagesAsync(chatRoomId, 50);
                
                var messageData = messages.Select(m => new
                {
                    id = m.Id,
                    chatRoomId = m.ChatRoomId,
                    senderId = m.SenderId,
                    senderName = m.Sender?.FullName ?? (m.SenderId == 0 ? "System" : "Unknown"),
                    content = m.Content,
                    sentAt = m.SentAt,
                    messageType = m.MessageType,
                    isRead = m.IsRead
                }).ToList();

                // Mark messages as read
                await _chatService.MarkMessagesAsReadAsync(chatRoomId, userId);

                return new JsonResult(messageData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for chat room {ChatRoomId}", chatRoomId);
                return BadRequest(new { error = "Failed to load messages" });
            }
        }

        public async Task<IActionResult> OnPostAssignStaffAsync([FromBody] AssignStaffRequest request)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 1;
                var user = await _userService.GetUserByIdAsync(userId);
                
                if (user == null || (user.Role != "Staff" && user.Role != "Admin"))
                {
                    return Forbid();
                }

                var staffId = request.StaffId ?? userId;
                
                // Verify staff member exists and has correct role
                var staff = await _userService.GetUserByIdAsync(staffId);
                if (staff == null || (staff.Role != "Staff" && staff.Role != "Admin"))
                {
                    return BadRequest(new { error = "Invalid staff member" });
                }

                await _chatService.AssignStaffToChatRoomAsync(request.ChatRoomId, staffId);
                
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning staff to chat room {ChatRoomId}", request.ChatRoomId);
                return BadRequest(new { error = "Failed to assign staff" });
            }
        }

        public async Task<IActionResult> OnPostCloseChatAsync([FromBody] CloseChatRequest request)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 1;
                var user = await _userService.GetUserByIdAsync(userId);
                
                if (user == null || (user.Role != "Staff" && user.Role != "Admin"))
                {
                    return Forbid();
                }

                // Verify access to chat room
                var chatRoom = await _chatService.GetChatRoomByIdAsync(request.ChatRoomId);
                if (chatRoom == null)
                {
                    return NotFound();
                }

                // Check permissions
                if (user.Role != "Admin" && chatRoom.StaffId != userId)
                {
                    return Forbid();
                }

                await _chatService.CloseChatRoomAsync(request.ChatRoomId);
                
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing chat room {ChatRoomId}", request.ChatRoomId);
                return BadRequest(new { error = "Failed to close chat" });
            }
        }

        public async Task<IActionResult> OnGetGetAvailableStaffAsync()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 1;
                var user = await _userService.GetUserByIdAsync(userId);
                
                if (user == null || user.Role != "Admin")
                {
                    return Forbid();
                }

                var availableStaff = await _chatService.GetAvailableStaffAsync();
                
                var staffData = availableStaff.Select(s => new
                {
                    id = s.Id,
                    fullName = s.FullName,
                    email = s.Email,
                    role = s.Role
                }).ToList();

                return new JsonResult(staffData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available staff");
                return BadRequest(new { error = "Failed to load staff" });
            }
        }

        public class AssignStaffRequest
        {
            public int ChatRoomId { get; set; }
            public int? StaffId { get; set; }
        }

        public class CloseChatRequest
        {
            public int ChatRoomId { get; set; }
        }
    }
}
